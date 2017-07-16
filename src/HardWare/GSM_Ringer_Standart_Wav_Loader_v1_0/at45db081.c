/*
  CodeVisionAVR C Compiler
  (C) 2005 Taras Drozdovsky 

  ATMEL AT45DB081 SPI functions
*/

#include <board.h>

void wait_end_page_operation(void){
 _CS=0;
 spi(0xD7);
 while(!(0x80&(spi(0xD7))));
 _CS=1;   
}
void AT45DB081_init(void){
_CS=1;
PORTB.SCK=1;
PORTB.MOSI=1;
DDRB.4=1;
DDRB.SCK=1;
DDRB.MOSI=1;  
wait_end_page_operation();
}
void page_to_buffer1(unsigned int paddr){
 unsigned char *addr;       
  addr=&paddr;
  _CS=0;
  spi(0x53);
  spi(*(addr+1));
  spi(*addr);
  spi(0x0);
  _CS=1;  
}
void buffer1_to_page(unsigned int paddr){
 unsigned char *addr;       
  addr=&paddr;
  _CS=0;
  spi(0x83);
  spi(*(addr+1));
  spi(*addr);
  spi(0x0);
  _CS=1;  
}
void page_to_buffer2(unsigned int paddr){
 unsigned char *addr;       
  addr=&paddr;
  _CS=0;
  spi(0x55);
  spi(*(addr+1));
  spi(*addr);
  spi(0x0);
  _CS=1;  
}
void buffer2_to_page(unsigned int paddr){
unsigned char *addr;       
  addr=&paddr;
  _CS=0;
  spi(0x86);
  spi(*(addr+1));
  spi(*addr);
  spi(0x0);
  _CS=1; 
}

void buffer1_write(unsigned char *data,unsigned int lenght,unsigned int baddr){
unsigned char *addr;
int i;       
 addr=&baddr;
 _CS=0;
 spi(0x84);
 spi(0x0);
 spi(*(addr+1));
 spi(*addr);
 for(i=0;i!=lenght;i++)
 spi(*(data+i));
 _CS=1;    
}
void buffer2_write(unsigned char *data,unsigned int lenght,unsigned int baddr){
unsigned char *addr;
int i;       
 addr=&baddr;
 _CS=0;
 spi(0x87);
 spi(0x0);
 spi(*(addr+1));
 spi(*addr);
 for(i=0;i!=lenght;i++)
 spi(*(data+i));    
 _CS=1;
 }
void buffer1_read(unsigned char *data,unsigned int lenght,unsigned int baddr){
unsigned char *addr;
int i;       
 addr=&baddr;
 _CS=0;
 spi(0x54);
 spi(0x0);
 spi(*(addr+1));
 spi(*addr);
 spi(0x0);
 for(i=0;i!=lenght;i++)
 *(data+i)=spi(0x0);    
 _CS=1;
}
void buffer2_read(unsigned char *data,unsigned int lenght,unsigned int baddr){
unsigned char *addr;
int i;       
 addr=&baddr;
 _CS=0;
 spi(0x56);
 spi(0x0);
 spi(*(addr+1));
 spi(*addr);
 spi(0x0);
 for(i=0;i!=lenght;i++)
 *(data+i)=spi(0x0);    
 _CS=1;
}
void write1(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){
 page_to_buffer1((paddr<<1));
 wait_end_page_operation();
 buffer1_write(&data[0],lenght,baddr);
 buffer1_to_page((paddr<<1));
 wait_end_page_operation();
} 
void write2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){
 page_to_buffer2((paddr<<1));
 wait_end_page_operation();
 buffer2_write(&data[0],lenght,baddr);
 buffer2_to_page((paddr<<1));
 wait_end_page_operation();
}   
void read1(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){  
 page_to_buffer1((paddr<<1));
 wait_end_page_operation();
 buffer1_read(&data[0],lenght,baddr);
 }
void read2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){  
 page_to_buffer2((paddr<<1));
 wait_end_page_operation();
 buffer2_read(&data[0],lenght,baddr);
 }
void write1sumb(char sumbol,unsigned int paddr){
int i;       
 _CS=0;
 spi(0x84);
 spi(0x0);
 spi(0x0);
 spi(0x0);
 for(i=0;i!=264;i++)
  spi(sumbol);
 _CS=1;    
 buffer1_to_page((paddr<<1));
 wait_end_page_operation();
}
void write2sumb(char sumbol,unsigned int paddr){
int i;       
 _CS=0;
 spi(0x87);
 spi(0x0);
 spi(0x0);
 spi(0x0);
 for(i=0;i!=264;i++)
  spi(sumbol);
 _CS=1;    
 buffer2_to_page((paddr<<1));
 wait_end_page_operation();
}
void write1FFpage(char sumbol,unsigned int paddr,unsigned int num){
int i;       
 _CS=0;
 spi(0x84);
 spi(0x0);
 spi(0x0);
 spi(0x0);
 for(i=0;i!=264;i++)
  spi(sumbol);
 _CS=1;   
 for(i=paddr;i!=(paddr+num);i++)
 {
  buffer1_to_page(i<<1);
  wait_end_page_operation();
 }
}
void mainread(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){  
 unsigned char *addr; 
 unsigned int i;      
 paddr<<=1;
 if(baddr&0x0100)
  paddr|=1;
 _CS=0;
 spi(0x52);
 addr=&paddr;
 spi(*(addr+1));
 spi(*addr);
 addr=&baddr;
 spi(*addr);
 spi(0x0);
 spi(0x0);
 spi(0x0);
 spi(0x0);
 for(i=0;i!=lenght;i++)
 *(data+i)=spi(0x0);    
 _CS=1;
 wait_end_page_operation();
}
void mainwrite2(unsigned char *data,unsigned int lenght,unsigned int baddr,unsigned int paddr){  
 unsigned char *addr; 
 unsigned int i;      
 paddr<<=1;
 if(baddr&0x0100)
  paddr|=1;
 _CS=0;
 spi(0x85);
 addr=&paddr;
 spi(*(addr+1));
 spi(*addr);
 addr=&baddr;
 spi(*addr);
 for(i=0;i!=lenght;i++)
  spi(*(data+i));    
 _CS=1;
 wait_end_page_operation();
}

