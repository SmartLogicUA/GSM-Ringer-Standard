#define RATE            115200L // 38400L
#define PAGESIZE        256
#define SYNC            0x00
#define MAXFADE         2048
#define F0      44100
#define FRC     2000 //5000 // 1000     /* 1/RC */
#define isrx    (inportb(port + 5) & 1)
#define getrx   (inportb(port))

char * szStereoMono[] = {"Mono","Stereo"};

unsigned port = 0x3F8;
short           data[4096];
unsigned char   page[PAGESIZE];
int     pagenum;
int     last;
int     nbuf = 0;
int     npage[2] = { 0, 0 };
int     fader = 0;
int     pagefader = 0;
typedef struct {
    //unsigned char       num;
    unsigned short      start;
    unsigned short      length;
    //unsigned char       res1;
    //unsigned char       res2;
    //unsigned char       res3;
} SNDTABLE;

typedef struct {
    unsigned long   dwRIFF;
    unsigned long   dwSize;
    unsigned long   dwWAVE;
    unsigned long   dw_fmt;
    unsigned long   dwOffset;
} RIFFFORMAT;

typedef struct {
    unsigned short  wFormatTag;       // see below
    unsigned short  nChannels;        // see below
    unsigned long   nSamplesPerSec;   // sample rate, in samples per second
    unsigned long   nAvgBytesPerSec;  // see below
    unsigned short  nBlockAlign;      // see below
} WAVEFORMAT;

typedef struct {
    unsigned short  wFormatTag;       // see below
    unsigned short  nChannels;        // see below
    unsigned long   nSamplesPerSec;   // sample rate, in samples per second
    unsigned long   nAvgBytesPerSec;  // see below
    unsigned short  nBlockAlign;      // see below
    unsigned short  wBitsPerSample;      // see below
} WAVEFORMATEX;

typedef struct {
    unsigned long   dwName;
    unsigned long   dwSize;
} SECTIONINFO;

RIFFFORMAT      rf;
WAVEFORMATEX    wf;
//SECTIONINFO   si;
SNDTABLE        stable[64];

/****************************************************************************/
void    wait(int tt)    // 50 ms intevals
{
  char t = (* ((char far *) 0x046C)) + 1 + tt;
  for(;* ((char far *) 0x046C) != t;);
}
/************************************** RX with 50-100 ms timeout ***********/
int     rx(void)
{
  int retv = -1;
  short time = * ((short far *) 0x046C);

  time += 2;
  while(time != * ((short far *) 0x046C))
    {
      if(isrx) return getrx;
    }
  return retv;
}
/***************************** TX byte & wait for transmission end **********/
void    tx(unsigned char byte)
{
#ifdef DEBUG
  printf(">%02X", byte);
#endif
  outportb(port, byte);
  while(! (inportb(port + 5) & 0x20));
}
/****************************************************************************/
void serial_init(void)
{
//----- Set Up Baud Rate
  outportb(port + 3, 0x80);
  outportb(port, 115200L / RATE);
  outportb(port + 1, 0);
//----- Set Up Mode (8 bit, 1 stop, no parity ???)
  outportb(port + 3, 0x03);
//  outportb(port + 3, 0x03 | 0x08);
//----- No interrupts
  outportb(port + 1, 0x00);
//----- Set Up DTS RTS
  outportb(port + 4, 0x02);
//  outportb(port + 4, 0x03);
//  outportb(port + 2, 0x00);     // Disaable FIFO
  outportb(port + 2, 0xC1);     // Disaable FIFO
}
/****************************************************************************/
void    sync(void)
{
  int   i;
  for(i=0; i<10; i++)
   tx(SYNC);
  tx(0xC3);
}
/****************************************************************************/
void    sync_long(void)
{
  int   i;
  for(i=0; i<1000; i++)
   tx(0x00);
  tx(0xC3);
}
/****************************************************************************/
void    load_page(void * page)
{
  int   i;
  char * p = page;
  printf("%d page\r", pagenum);
  if(nbuf)
    {
      sync();
      tx(0x84);
      tx(0);
      tx(0);
      tx(0);
      for(i=0; i<PAGESIZE; i++)
          tx(p[i]);
      sync();
      tx(0x83);
      tx(pagenum>>7);
      tx(pagenum<<1);
      tx(0);
      nbuf = 0;
    }
  else
    {
      sync();
      tx(0x87);
      tx(0);
      tx(0);
      tx(0);
      for(i=0; i<PAGESIZE; i++)
          tx(p[i]);
      sync();
      tx(0x86);
      tx(pagenum>>7);
      tx(pagenum<<1);
      tx(0);
      nbuf = 1;
    }
  pagenum ++;
}
/****************************************************************************/
/****************************************************************************/
int     load_file(char * fname)
{
  int pages = 0;
  int hIn;
  int len;
  int pos = 0;
  long sample;
  int  delta;
  long x;

  fader = 0;
  if((hIn = _open(fname, 0)) != -1)
    {
      _read( hIn, & rf, sizeof(rf) );
      _read( hIn, & wf, sizeof(wf) );
      if(rf.dwWAVE != 0x45564157)
            printf("ERROR: Bad WAV file!\n");
      else
        {
          printf("Loading '%s' ", fname);
          printf("(%ld Hz, %d bits, %s)\n",wf.nSamplesPerSec,wf.wBitsPerSample,szStereoMono[wf.nChannels-1]);
          while( (len = _read(hIn, data, sizeof(data))) != 0)
            {
              unsigned i;
              unsigned char * p = (char *) data;
                if(kbhit())
                {
                 bioskey(0);
                 break;
                }
              for(i=0; i<(len); )
                {
                  int j;
                  sample = 0;
                  for( j=0; j<wf.nChannels; j++ )
                    {
                      if(wf.wBitsPerSample == 16)
                        {
                          sample += * (short *)(& p[i]);
                          i += 2;
                        }
                      else sample += ((signed long)p[i++] - 0x80) << 8;
                    }
                  sample /= wf.nChannels;

                  if(fader)
                    {
                      sample = (sample * fader) / MAXFADE;
                      if(fader != 1)
                          fader --;
                    }
                  sample >>= 1;
                  x = ((sample - last) * F0) / FRC + last;
                  if(x > 0x7FFF) x = 0x7FFF;
                  if(x < -0x7FFF) x = -0x7FFF;
                  x &= 0xFFFFFF00;
                  last = last + ((x - last) * FRC) / F0;
                  page[pos ++] = (x>>8) + 0x80;
                  if(pos >= sizeof(page))
                    {
                      load_page(page);
                      pos = 0;
                      pages ++;
                      if(pages == pagefader)
                        {
                          printf("\t\t\t\t* FadeOut %d page\n", pages);
                          fader = MAXFADE;
                        }
                    }
                }
            }
          if(pos)
            {
              for(; pos<sizeof(page); pos ++)
                  page[pos] = 0x80;
              load_page(page);
            }
          _close( hIn );
        }
    }
  else
    {
      printf("ERROR: unable to open '%s'\n", fname);
    }
  return pages;
}
/****************************************************************************/
//int main( int argv, char * argc[] )
int main( )
{
  int   i, len;
  printf(
  "컴컴컴컴컴컴컴컴�-컴컴컴컴컴\n"
  "Sound Loader by T.Drozdovsky\n"
  "컴컴컴컴컴컴컴컴컴컴컴컴컴컴\n" );

  serial_init();
  wait(10);
  sync_long();
  pagenum = 0;//16;
  for(i=0; i!=36; i++)
    {
      static char fname[256];
      sync_long();
      stable[i].start = (pagenum + 1) ;//<< 1;
      sprintf(fname, "%02X.wav", i);
      last = 0;
      len = load_file(fname);
      printf("\t\t\t\t* Written %d pages (%d KBytes)\n", len, len >> 2);
      if(len)
        {
          stable[i].length = len - 1;
          //stable[i].num = 1;
        }
      else
        {
          //stable[i].num = 0;
          stable[i].start = 0;
          stable[i].length = 0;
        }
    }
  printf("A total of %d pages (%d KBytes) written\n", pagenum, pagenum >> 2);
  printf("Writing SOUND TABLE:\n");
  pagenum = 0;
  load_page(stable);
  printf("Ok...........\n");
  sync();
  tx(0xFF);
  while(! (inportb(port + 5) & 0x40)); // Wait For Transmitter Empty
return ;
}

/****************************************************************************/
