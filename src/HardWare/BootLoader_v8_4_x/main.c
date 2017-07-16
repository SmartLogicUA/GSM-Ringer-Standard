/*****************************************************
This program was produced by the
CodeWizardAVR V1.25.3 Professional
Automatic Program Generator
© Copyright 1998-2007 Pavel Haiduc, HP InfoTech s.r.l.
http://www.hpinfotech.com

Project : BootLoader
Version : v1_0
Date    : 26.09.2009
Author  : T Drozdovskiy                   
Company : Smart Logic                     
Comments: 

Chip type           : ATmega32
Program type        : Boot Loader - Size:2048words
Clock frequency     : 11,059200 MHz
Memory model        : Small
External SRAM size  : 0
Data Stack size     : 512
*****************************************************/

#include <mega32.h>
#include <string.h>    
#include <board.h>
#include <g28147.h>     

#define SPMEN  0
#define PGERS  1
#define PGWRT  2
#define BLBSET 3
#define RWWSRE 4
#define RWWSB  6
#define SPMIE  7
#define EEWE   1 
#define EEPE   1                          

/*****************************************************************************/
/*                G L O B A L    V A R I A B L E S                           */
/*****************************************************************************/

flash char FHandContr[]="SYHND 00004";                    // Get Handle Controller
#define VERSIA_BOOT_LOADER  "\r\nBootLoader v8.4.x"

#pragma warn-

register unsigned char  Bufferl  @0;
register unsigned char  Bufferh  @1;

register unsigned int  reg_temp @2;
flash unsigned char *pFlash;
eeprom unsigned char *pEEPROM;

union {                                                 // union for conversion type long,int in char
  unsigned char byte[4];
  unsigned int word[2];
  unsigned long dword;      
 } number;

union {                                                 // union for check sum (interrupt)
 unsigned char crcb[2];
 unsigned int crcw;      
      } crci;      

unsigned char kkkk[]="CRC ERROR"; //"Not correct CRC check sum";

#pragma warn+ 


unsigned int counter_PC=0;
unsigned char buffer_PC[275];

unsigned char StartFlagPC=0;        //  bit

unsigned char BootHere[]="SBLFL";   


/*****************************************************************************/
/*                 G O S T   2 8 1 4 7 - 8 9                                 */
/*****************************************************************************/

unsigned long S[2];
unsigned long KZU[8]={0x9EFD2C00,0xF5E42190,0x3E026689,0xF94B77F6,  // KEY
                      0xD01E3E0F,0x80FDA068,0xE227DAC7,0x041CB3CA};

unsigned char K[128]={       // Gam_c
   0x0F, 0x04, 0x03, 0x06, 0x0D, 0x02, 0x09, 0x08, 0x00, 0x0C, 0x01, 0x0E, 0x07, 0x05, 0x0A, 0x0B,
   0xE0, 0x30, 0xC0, 0x20, 0xD0, 0x90, 0x10, 0xB0, 0x50, 0x40, 0x60, 0xA0, 0x00, 0xF0, 0x80, 0x70,
   0x04, 0x08, 0x0C, 0x01, 0x0E, 0x07, 0x00, 0x0F, 0x0B, 0x02, 0x09, 0x06, 0x05, 0x03, 0x0A, 0x0D,
   0x10, 0x90, 0x80, 0x50, 0xE0, 0xF0, 0xA0, 0xB0, 0x00, 0xC0, 0x70, 0x30, 0xD0, 0x60, 0x20, 0x40,
   0x09, 0x03, 0x04, 0x0C, 0x01, 0x00, 0x0B, 0x05, 0x06, 0x0F, 0x0D, 0x0E, 0x07, 0x02, 0x0A, 0x08,
   0xE0, 0x40, 0x90, 0xD0, 0xA0, 0x30, 0xF0, 0x10, 0xB0, 0x70, 0x80, 0x50, 0x60, 0xC0, 0x20, 0x00,
   0x02, 0x0A, 0x06, 0x0E, 0x05, 0x0D, 0x0C, 0x03, 0x07, 0x08, 0x0F, 0x00, 0x09, 0x0B, 0x01, 0x04,
   0xB0, 0x90, 0x80, 0x10, 0xA0, 0x60, 0xD0, 0x40, 0x00, 0x50, 0x20, 0xC0, 0xE0, 0xF0, 0x30, 0x70};

/*****************************************************************************/
/*                  F U N C T I O N  P R O T O T Y P E                       */
/*****************************************************************************/
void Receive_PC(void);
void ProgramFlashPage(unsigned char *);
void ProgramEEPROM(unsigned char *);
void InitSign(unsigned char *);
void SendAnswerR(char *);
void Print(flash char *data);
        
void crc2(unsigned char *,unsigned int *,unsigned int);
unsigned char check_flash_crc(void);
void WriteFlash(unsigned int P_address,unsigned char *pData);  
void ASCIIToHex(unsigned char *,unsigned int);
/*****************************************************************************/  

flash char tascii[]={"0123456789ABCDEF"};  

#pragma warn+ 

#define TOIE0   (1<<0)
#define OCIE0   (1<<1)
#define TOIE1   (1<<2)
#define OCIE1B  (1<<3)
#define TOIE2   (1<<6)
#define OCIE2   (1<<7)

#define TOV0    (1<<0)
#define OCF0    (1<<1)
#define TOV1    (1<<2)
#define OCF1B   (1<<3)
#define TOV2    (1<<6)
#define OCF2    (1<<7)
        
#define INT0    (1<<6)
#define INTF0   (1<<6)
#define INT1    (1<<7)
#define INTF1   (1<<7)
#define INT2    (1<<5)
#define INTF2   (1<<5)

// PUSART1 Receiver buffer
#define RX_BUFFER_SIZE1 128
char rx_buffer1[RX_BUFFER_SIZE1];
#if RX_BUFFER_SIZE1<256
unsigned char rx_wr_index1,rx_rd_index1,rx_counter1;
#else
unsigned int rx_wr_index1,rx_rd_index1,rx_counter1;
#endif

// This flag is set on PUSART1 Receiver buffer overflow
//unsigned char rx_buffer_overflow1; //bit

unsigned char UartswRxData;
unsigned char UartswRxBitNum;
char getchar1(void);

// External Interrupt 0 service routine
interrupt [EXT_INT0] void ext_int0_isr(void)
{
// Place your code here
 GICR&=~INT0;
 OCR2=TCNT2+13;//27;          
 TIFR|=OCF2;
 TIMSK|=OCIE2;
 UartswRxBitNum=0;
 UartswRxData=0;
}
// Timer 2 output compare interrupt service routine
interrupt [TIM2_COMP] void timer2_comp_isr(void)
{
// Place your code here
 UartswRxData >>=1;
 if(PIND.2) UartswRxData |= 0x80;
 UartswRxBitNum++;
 OCR2+=9;//18;          
 if(UartswRxBitNum >= 8)
 {               
    rx_buffer1[rx_wr_index1]=UartswRxData;
    if (++rx_wr_index1 == RX_BUFFER_SIZE1)rx_wr_index1=0;
    if (++rx_counter1 == RX_BUFFER_SIZE1)
    {
        rx_counter1=0;
        //rx_buffer_overflow1=1;
    }
    TIMSK&=~OCIE2; 
    GIFR|=INTF0;
    GICR|=INT0;
 }
}

char getchar1(void)
{
char data;
while (rx_counter1==0);
data=rx_buffer1[rx_rd_index1];
if (++rx_rd_index1 == RX_BUFFER_SIZE1) rx_rd_index1=0;
#asm("cli")
--rx_counter1;
#asm("sei")
return data;
}


// USART Transmitter buffer
#define TX_BUFFER_SIZE1 128
char tx_buffer1[TX_BUFFER_SIZE1];

#if TX_BUFFER_SIZE1<256
unsigned char tx_wr_index1,tx_rd_index1,tx_counter1;
#else
unsigned int tx_wr_index1,tx_rd_index1,tx_counter1;
#endif

unsigned char UartswTxData;
unsigned char UartswTxBitNum;
unsigned char UartswTxBusy=0; // bit

// Timer 0 output compare interrupt service routine
interrupt [TIM0_COMP] void timer0_comp_isr(void)
{
// Place your code here
	if(UartswTxBitNum)
	{
		if(UartswTxBitNum > 1)
		{
			if(UartswTxData & 0x01) PORTD.6=1;
			else PORTD.6=0;
			UartswTxData>>=1;
		}
		else
		{                                     
		    PORTD.6=1;
		}
		UartswTxBitNum--;
		OCR0+=9;//18;
	}
	else
	{                  
	    if (tx_counter1)
        {
            --tx_counter1;
            UartswTxData = tx_buffer1[tx_rd_index1];;
	        UartswTxBitNum = 9;	
            OCR0=TCNT0+9;//18;
            PORTD.6=0;
            if (++tx_rd_index1 == TX_BUFFER_SIZE1) tx_rd_index1=0;
        }
        else 
        {
            UartswTxBusy = 0;
            TIFR|= OCF0;
            TIMSK|=OCIE0;            
        }    
	}
}
                 
void putchar1(char c)
{
while (tx_counter1 == TX_BUFFER_SIZE1);
 #asm("cli")
if (tx_counter1 || (UartswTxBusy))
   {
   tx_buffer1[tx_wr_index1]=c;
   if (++tx_wr_index1 == TX_BUFFER_SIZE1) tx_wr_index1=0;
   ++tx_counter1;
   }
else
   {
    UartswTxBusy = 1;
	UartswTxData = c;
	UartswTxBitNum = 9;	
    OCR0=TCNT0+9;//18;   
    TIFR|=OCF0;
    TIMSK|=OCIE0;
    PORTD.6=0;
   }
#asm("sei")
}          

// Timer 1 output compare A interrupt service routine
interrupt [TIM1_COMPA] void timer1_compa_isr(void)
{
// Place your code here
    PORTC^=0xF8;
}

// Declare your global variables here

void main(void)
{
// Declare your local variables here
unsigned char i;
// Input/Output Ports initialization
// Port A initialization
// Func7=In Func6=In Func5=In Func4=In Func3=In Func2=In Func1=In Func0=In 
// State7=T State6=T State5=T State4=T State3=T State2=T State1=T State0=T 
PORTA=0x00;
DDRA=0x00;

// Port B initialization
// Func7=In Func6=In Func5=In Func4=In Func3=In Func2=In Func1=In Func0=In 
// State7=T State6=T State5=T State4=T State3=T State2=T State1=T State0=T 
PORTB=0x00;
DDRB=0x00;

// Port C initialization
// Func7=Out Func6=Out Func5=Out Func4=Out Func3=Out Func2=In Func1=In Func0=In 
// State7=1 State6=1 State5=1 State4=1 State3=1 State2=T State1=T State0=T 
PORTC=0xF8;
DDRC=0xF8;

// Port D initialization
// Func7=In Func6=Out Func5=In Func4=In Func3=In Func2=In Func1=In Func0=In 
// State7=T State6=1 State5=T State4=T State3=T State2=P State1=T State0=T 
PORTD=0x44;
DDRD=0x40;

// Timer/Counter 0 initialization
// Clock source: System Clock
// Clock value: 172,800 kHz
// Mode: CTC top=OCR0
// OC0 output: Disconnected
TCCR0=0x03;
TCNT0=0x00;
OCR0=0x00;

// Timer/Counter 1 initialization
// Clock source: System Clock
// Clock value: 10,800 kHz
// Mode: CTC top=OCR1A
// OC1A output: Discon.
// OC1B output: Discon.
// Noise Canceler: Off
// Input Capture on Falling Edge
// Timer 1 Overflow Interrupt: Off
// Input Capture Interrupt: Off
// Compare A Match Interrupt: On
// Compare B Match Interrupt: On
TCCR1A=0x00;
TCCR1B=0x0D;
TCNT1H=0x00;
TCNT1L=0x00;
ICR1H=0x00;
ICR1L=0x00;
OCR1AH=0x08;
OCR1AL=0x00;
OCR1BH=0x00;
OCR1BL=0x00;

// Timer/Counter 2 initialization
// Clock source: System Clock
// Clock value: 172,800 kHz
// Mode: CTC top=OCR2
// OC2 output: Disconnected
ASSR=0x00;
TCCR2=0x04;
TCNT2=0x00;
OCR2=0x00;

// External Interrupt(s) initialization
// INT0: On
// INT0 Mode: Falling Edge
// INT1: Off
// INT2: Off
GICR|=0x40;
MCUCR=0x02;
MCUCSR=0x00;
GIFR=0x40;

// Timer(s)/Counter(s) Interrupt(s) initialization
TIMSK=0x10;

// Analog Comparator initialization
// Analog Comparator: Off
// Analog Comparator Input Capture by Timer/Counter 1: Off
ACSR=0x80;
SFIOR=0x00;

// Watchdog Timer initialization
// Watchdog Timer Prescaler: OSC/256k
#pragma optsize-
WDTCR=0x1F;
WDTCR=0x0F;
#ifdef _OPTIMIZE_SIZE_
#pragma optsize+
#endif
#asm("wdr")
// Global enable interrupts
#asm("sei")

Print(VERSIA_BOOT_LOADER);

for(i=0;i!=12;i++)
{
 if(FHandContr[i]!=(*(pEEPROM+HND+i)))
  *(pEEPROM+HND+i)=FHandContr[i];
}             
Print("\r\nS/N ");
Print(&FHandContr[6]);

if(check_flash_crc())
{
 Print("\r\nCRC OK\r\n\n");
 while(tx_counterPC)
 #asm("wdr");
 #asm("cli")
 MCUCR=0x01;                         //interrupts vectors are in the boot sector
 MCUCR=0x00;
 #asm("jmp 0x000");                 // Run application code
}
else 
{
 Print("\r\nCRC ERROR\r\n"); 
}   

SendAnswerR(BootHere);
   
while (1)                                                          
      { 
      // Place your code here
      #asm("wdr")
      if(rx_counterPC) Receive_PC();
      };
}
void WriteFlash(unsigned int P_address,unsigned char *pData)
{ 
 unsigned char i;
 #asm("wdr")
 #asm("cli");            
 while(EECR & (1<<EEPE));
 reg_temp = P_address<<7;
 SPMCR|=(1<<PGERS) | (1<<SPMEN);
 #asm  
     mov r31,r3
     mov r30,r2
     spm
 #endasm
 while(SPMCR&(1<<SPMEN));        // Polled to find out when the CPU is ready for further page updates
 SPMCR|=(1<<RWWSRE)|(1<<SPMEN);  // RWWSB Flag cleared by software when the SPM operation is completed.
 #asm("spm")
 #asm("sei")
 for (i=0;i!=128;i+=2)
 { 
  #pragma warn-         
  Bufferh=*(pData+i+1);
  Bufferl=*(pData+i);
  reg_temp = i;
  SPMCR =(1<<SPMEN);
  #asm  
     mov r31,r3
     mov r30,r2
     spm
  #endasm
  #pragma warn+
 }
 #asm("cli")
 reg_temp = P_address<<7;;
 SPMCR|=(1<<PGWRT) | (1<<SPMEN);
 #asm  
    mov r31,r3
    mov r30,r2
    spm
 #endasm
 while(SPMCR&(1<<SPMEN));        // Polled to find out when the CPU is ready for further page updates
 SPMCR|=(1<<RWWSRE)|(1<<SPMEN);  // RWWSB Flag cleared by software when the SPM operation is completed.
 #asm("spm");
 #asm("sei");
}
unsigned char check_flash_crc(void)
{
 unsigned char mas[128];
 unsigned char i;  
 unsigned int j;
 unsigned int k;
 #asm("wdr") 
 crci.crcw=0;
 k=(*(pEEPROM+LENG+1));
 for(j=0;j!=k;j++)
 {
  for(i=0;i!=128;i++)
   mas[i]=*(pFlash+i+j*128);
  crc2(&mas[0],&crci.crcw,128);
 }              
 if((crci.crcb[1]==(*(pEEPROM+CRCEL)))&&(crci.crcb[0]==(*(pEEPROM+CRCEH))))
 {
  return 1;
 }
 return 0;
}
/**
 * print with Flash memory string.  
 *
 * @param	*data	a pointer to the string command 
 **/
void Print(flash char *data){
 unsigned char i=0;
 do {     
   #asm("wdr")
   putcharPC(data[i]);
  }while(data[++i]); 
}
/**
 * Calculated CRC checksum 
 *
 * @param	*ZAdr	a pointer to the data 
 * @param	*DoAdr	a pointer to the CRC checksum returned
 * @param	lle	amount byte need for calculate CRC checksum
 **/
void crc2(unsigned char *ZAdr,unsigned int *DoAdr,unsigned int lle){
 unsigned char i;
 unsigned int C,NrBat;
 #asm("wdr")
 for(NrBat=0;NrBat!=lle;NrBat++,ZAdr++)
 {          
  C=((*DoAdr>>8)^*ZAdr)<<8;
  for(i=0;i!=8;i++)
   if (C&0x8000)
    C=(C<<1)^0x1021;
   else C=C<<1;
    *DoAdr=C^(*DoAdr<<8);
 }        
}   

void mmm(unsigned char *pData)
{
 unsigned char i;
 unsigned char a;
 #asm("wdr") 
 ASCIIToHex(&pData[0],128);
 for(i=0;i!=32;i++)
 {           
  a=pData[i*4];
  pData[i*4]=pData[i*4+3];
  pData[i*4+3]=a;
  a=pData[i*4+1];
  pData[i*4+1]=pData[i*4+2];
  pData[i*4+2]=a;  
 }  
 Gam_cD(&pData[0],&KZU[0],&K[0],16);
 for(i=0;i!=32;i++)
 {           
  a=pData[i*4];
  pData[i*4]=pData[i*4+3];
  pData[i*4+3]=a;
  a=pData[i*4+1];
  pData[i*4+1]=pData[i*4+2];
  pData[i*4+2]=a;  
 }  
}

void ProgramFlashPage(unsigned char *mas)
{           
 unsigned char kkk[]="SFLSH0000OK";
 #asm("wdr")
 kkk[5]=mas[0];
 kkk[6]=mas[1];
 kkk[7]=mas[2];
 kkk[8]=mas[3];                 
 ASCIIToHex(&mas[0],2);
 mmm(&mas[4]);
 WriteFlash(mas[1],&mas[4]); 
 SendAnswerR(kkk);
}
/**
 * Send answer with Flash memory string.  
 *
 * @param	*data	a pointer to the string command 
 **/
void SendAnswerR(char *data){
 unsigned char i=0;
 unsigned char checkbyte=0;
 #asm("wdr")
 putcharPC('$');
 do {     
   #asm("wdr")
   checkbyte^=data[i];
   putcharPC(data[i]);
  }while(data[++i]); 
   putcharPC('*');
   putcharPC(tascii[checkbyte>>4]);
   putcharPC(tascii[checkbyte&0x0F]);
   putcharPC('\n');     
}

/*****************************************************************************/ 
void ProgramEEPROM(unsigned char *mas)
{	    
 unsigned int addr;
 unsigned int lenght;
 unsigned int i;  
 unsigned char kkk[]="SEEPR0000OK";
 #asm("wdr")
 kkk[5]=mas[0];
 kkk[6]=mas[1];
 kkk[7]=mas[2];
 kkk[8]=mas[3];
 ASCIIToHex(&mas[0],4);
 addr=mas[0];
 addr<<=8;
 addr|=mas[1];       
 lenght=mas[2];
 lenght<<=8;
 lenght|=mas[3];
 ASCIIToHex(&mas[8],lenght);
 for(i=0;i!=lenght;i++)
 {                
  *(pEEPROM+addr+i)=mas[i+8];
 }
 SendAnswerR(kkk);
 if(addr==LENG)
 {
    while(tx_counterPC)
    #asm("wdr");
    #asm("cli")
    while(1);
 }                                         
}

/**
 * Receive data from PC and shaping packet  
 **/
void Receive_PC(void)
{
 unsigned char data;     
 data=getcharPC();
  switch (data) {
    case '$': { StartFlagPC=1; counter_PC=0;}
    break;
    case '\n': 
        if(StartFlagPC)
        {
         StartFlagPC=0;
         data=0;                  
         counter_PC=0;
         while((buffer_PC[counter_PC] != '*')&&(buffer_PC[counter_PC] != '\n') )	  
         data^=buffer_PC[counter_PC++];                      
         if(buffer_PC[counter_PC]=='*')
         {      
          if((tascii[data>>4]==buffer_PC[++counter_PC])&&(tascii[data&0x0F]==buffer_PC[++counter_PC]))

            {
                if(!strncmpf(buffer_PC, "PSIGN", 5))
	            {
	                InitSign(&buffer_PC[5]); 
	            }
	            if(!strncmpf(buffer_PC, "PFLSH", 5))
	            {         
	                ProgramFlashPage(&buffer_PC[5]);
	            }
 	            if(!strncmpf(buffer_PC, "PBLFL", 5))
 	            {         
                    Gost_init(&S[0]);
                    SendAnswerR(BootHere);	    
 	            }
                if(!strncmpf(buffer_PC, "PEEPR", 5))
	            {
	                ProgramEEPROM(&buffer_PC[5]);
                } 
            }
            else
            {
                SendAnswerR(kkkk);
            }
         } 
         else
         {
          ;//SendAnswerR(kkkk);
         }
    }
    break;
    default: 
        if(StartFlagPC) buffer_PC[counter_PC++]=data;  
    };
}                                   

void InitSign(unsigned char *mas)
{
 unsigned char answerC[]="SSIGNOK";
 #asm("wdr")
 ASCIIToHex(&mas[0],8);
 number.byte[3]=mas[0];
 number.byte[2]=mas[1];
 number.byte[1]=mas[2];
 number.byte[0]=mas[3];
 S[0]=number.dword;
 number.byte[3]=mas[4];
 number.byte[2]=mas[5];
 number.byte[1]=mas[6];
 number.byte[0]=mas[7];
 S[1]=number.dword;
 Gost_init(&S[0]);
 SendAnswerR(answerC);
}

void ASCIIToHex(char *mas,unsigned int num)
{
 unsigned char a,b;
 unsigned int i,j;
 #asm("wdr")
 for(i=0,j=0;j!=num;i+=2,j++)
 {             
    a=mas[i]-0x30;
    if(a>9) a-=7;
    a<<=4;
    b=mas[i+1]-0x30;
    if(b>9)b-=7;
    mas[j]=a|b;
 }
}
