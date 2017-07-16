/*
  CodeVisionAVR C Compiler
  (C) 2009 Taras Drozdovsky, My.
*/

#include <board.h>

inline void Sound_ISR(void)
{
  OCREG=sndb;
  if(FSND)
  {           
   if(!cnt0)
   {   
    current_buffer=~current_buffer; 
    if(len)
    { 
     if(current_buffer)need_read_buffer2=1;
     else need_read_buffer1=1;
     SoftInterrupt|=SI_UPBW;
     SI_GEN     
    }
    else
    {
     FSND=0;
     OCREG=0x80;  
     SoftInterrupt|=SI_PLAY_LIST;
     SI_GEN     
    }
   }
   if(current_buffer)
       sndb=buffer1[cnt0++];
   else
       sndb=buffer2[cnt0++];
   if(cnt0==128) cnt0=0; 
  } 
} 

void update_snd_buffer(void)
{           
     #asm("wdr")
      if(need_read_buffer1)
      {
       if(len)
       {
        read2(&buffer1[0],128,0,addr_page);
        //len--;
       } 
       need_read_buffer1=0;                    
      }
      if(need_read_buffer2) 
      {
       if(len)
       {
        read2(&buffer2[0],128,128,addr_page++);
        len--;
       } 
       need_read_buffer2=0;                    
      }
}
// void update_snd_buffer(void)
// {           
//      #asm("wdr")
//       if(need_read_buffer1)
//       {
//        if(len)
//        {
//         read2(&buffer1[0],512,0,addr_page++);
//         len--;
//        } 
//        need_read_buffer1=0;                    
//       }
//       if(need_read_buffer2) 
//       {
//        if(len)
//        {
//         read2(&buffer2[0],512,0,addr_page++);
//         len--;
//        } 
//        need_read_buffer2=0;                    
//       }
// }

void snd_play(unsigned char num)
{
 FSND=0; 
 need_read_buffer1=1;                    
 need_read_buffer2=1;                    
 addr_page=stable[num].start;
 len=stable[num].lenght;
 cnt0=0;
 OCREG=sndb=0x80;
 current_buffer=1;
 FSND=1; 
}

void pc_comm(void)
{          
 unsigned char tmp;
 unsigned int i;
 //TIMSK=0x0;
 FSND=0;
 //getcharM();
 //getcharM();
 //tmp=getcharM();
 if(tmp!=0x00)
  while(1);
 while(1)
 {
  do{
  #asm("wdr")
  //tmp=getcharM();
  }while(tmp==0x00);
  if(tmp!=0xC3)
   while(1);
  #asm("wdr")
 //tmp=getcharM();
 if(tmp!=0x00)
 {
  switch (tmp) {
    case 0x84: {          
              LED_ON  
              _CS=0;
              spi(0x84);
 //             spi(getcharM());
 //             spi(getcharM());
 //             spi(getcharM());
              for(i=0;i<512;i++)
 //              spi(getcharM());
              _CS=1;
              }
    break;
    case 0x87: {  
              LED_ON  
              _CS=0;
              spi(0x87);
   //           spi(getcharM());
   //           spi(getcharM());
   //           spi(getcharM());
              for(i=0;i<512;i++)
   //            spi(getcharM());
              _CS=1;
              }
    break;
    case 0x83:{     
              LED_OFF  
              _CS=0;
              spi(0x83);
    //          spi(getcharM());
    //          spi(getcharM());
    //          spi(getcharM());
              _CS=1;
              }
    break;
    case 0x86:{      
              LED_OFF  
              _CS=0;
              spi(0x86);
    //          spi(getcharM());
    //          spi(getcharM());
    //          spi(getcharM());
              _CS=1;
              }
    break;
    case 0x58:{
              _CS=0;
              spi(0x58);
    //          spi(getcharM());
    //          spi(getcharM());
    //          spi(getcharM());
              _CS=1;
              }        
    
    break;
    case 0x59:{
              _CS=0;
              spi(0x59);
     //         spi(getcharM());
     //         spi(getcharM());
     //         spi(getcharM());
              _CS=1;
              }
    break;
    case 0x82:{
              _CS=0;
              spi(0x82);
     //         spi(getcharM());
     //         spi(getcharM());
     //         spi(getcharM());
              _CS=1;
              }
    break;
    case 0x85:{
              _CS=0;
              spi(0x85);
     //         spi(getcharM());
     //         spi(getcharM());
     //         spi(getcharM());
              _CS=1;
              }
    break;
    case 0xAA:
    break;
    };  
   }   
  }
}        
