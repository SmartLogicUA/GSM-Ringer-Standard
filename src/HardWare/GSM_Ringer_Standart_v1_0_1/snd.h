/*
  CodeVisionAVR C Compiler
  (C) 2009 Taras Drozdovsky, My.
*/
   
#ifndef _SND_INCLUDED_
#define _SND_INCLUDED_

#include <board.h>
#include <spi.h>

#define SND_BUFFER_SIZE 128
        
#define snd_stop()           FSND=0; OCR1AL=sndb=0x80              
#define snd_load_info()      FSND=0; read1(&stable[0],NUMMELODY*4,0,0)

#pragma used+

bit FSND=0;
bit current_buffer=0;
bit need_read_buffer1=0;
bit need_read_buffer2=0;

unsigned char sndb=0;
unsigned char cnt0=0;
unsigned int addr_page;
unsigned int len;          

unsigned char buffer1[SND_BUFFER_SIZE];
unsigned char buffer2[SND_BUFFER_SIZE];

typedef struct {
    unsigned int	start;
    unsigned int	lenght;
} SNDTABLE;

SNDTABLE	stable[NUMMELODY];

void snd_play(unsigned char);
void update_snd_buffer(void);
void pc_comm(void);

inline void Sound_ISR(void);

#pragma used-
#endif
