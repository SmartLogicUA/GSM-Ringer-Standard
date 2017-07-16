/*
  CodeVisionAVR C Compiler
  (C) 2005 Taras Drozdovsky, My.
*/
   
#ifndef _AT45DB081_INCLUDED_
#define _AT45DB081_INCLUDED_
#include <delay.h> 
#include <spi.h>
#pragma used+

void AT45DB081_init(void);
void page_to_buffer1(unsigned int paddr);
void buffer1_to_page(unsigned int paddr);
void page_to_buffer2(unsigned int paddr);
void buffer2_to_page(unsigned int paddr);
void wait_end_page_operation(void);
void buffer1_write(unsigned char *data,unsigned int lenght,unsigned int baddr);
void buffer2_write(unsigned char *data,unsigned int lenght,unsigned int baddr);
void buffer1_read(unsigned char *data,unsigned int lenght,unsigned int baddr);
void buffer2_read(unsigned char *data,unsigned int lenght,unsigned int baddr);   
void write1sumb(char sumbol,unsigned int paddr);
void write2sumb(char sumbol,unsigned int paddr);
void write1(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);
void write2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);
void read1(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);
void read2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);
void write1FFpage(char sumbol,unsigned int paddr,unsigned int num);
void mainread(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);
void mainwrite2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr);

#pragma used-
#endif
