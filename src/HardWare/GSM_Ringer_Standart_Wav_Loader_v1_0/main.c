/*****************************************************
This program was produced by the
CodeWizardAVR V1.25.3 Professional
Automatic Program Generator
© Copyright 1998-2007 Pavel Haiduc, HP InfoTech s.r.l.
http://www.hpinfotech.com

Project : GSM-Ringer Standart
Version : v1.0
Date    : 22.09.2009
Author  : T Drozdovskiy
Company : Smart Logic
Comments: 

Chip type           : ATmega32
Program type        : Application
Clock frequency     : 11,059200 MHz
Memory model        : Small
External SRAM size  : 0
Data Stack size     : 512
*****************************************************/

#include <mega32.h>
#include <board.h>
#include <delay.h>                     
#include <at45db081.h>

#define RXB8 1
#define TXB8 0
#define UPE 2
#define OVR 3
#define FE 4
#define UDRE 5
#define RXC 7

#define FRAMING_ERROR (1<<FE)
#define PARITY_ERROR (1<<UPE)
#define DATA_OVERRUN (1<<OVR)
#define DATA_REGISTER_EMPTY (1<<UDRE)
#define RX_COMPLETE (1<<RXC)

// USART Receiver buffer
#define RX_BUFFER_SIZE 128
char rx_buffer[RX_BUFFER_SIZE];

#if RX_BUFFER_SIZE<256
unsigned char rx_wr_index,rx_rd_index,rx_counter;
#else
unsigned int rx_wr_index,rx_rd_index,rx_counter;
#endif

// This flag is set on USART Receiver buffer overflow
bit rx_buffer_overflow;

// USART Receiver interrupt service routine
interrupt [USART_RXC] void usart_rx_isr(void)
{
char status,data;
status=UCSRA;
data=UDR;
if ((status & (FRAMING_ERROR | PARITY_ERROR | DATA_OVERRUN))==0)
   {
   rx_buffer[rx_wr_index]=data;
   if (++rx_wr_index == RX_BUFFER_SIZE) rx_wr_index=0;
   if (++rx_counter == RX_BUFFER_SIZE)
      {
      rx_counter=0;
      rx_buffer_overflow=1;
      };
   };
}

#ifndef _DEBUG_TERMINAL_IO_
// Get a character from the USART Receiver buffer
#define _ALTERNATE_GETCHAR_
#pragma used+
char getchar(void)
{
char data;
while (rx_counter==0);
data=rx_buffer[rx_rd_index];
if (++rx_rd_index == RX_BUFFER_SIZE) rx_rd_index=0;
#asm("cli")
--rx_counter;
#asm("sei")
return data;
}
#pragma used-
#endif

// USART Transmitter buffer
#define TX_BUFFER_SIZE 32
char tx_buffer[TX_BUFFER_SIZE];

#if TX_BUFFER_SIZE<256
unsigned char tx_wr_index,tx_rd_index,tx_counter;
#else
unsigned int tx_wr_index,tx_rd_index,tx_counter;
#endif

// USART Transmitter interrupt service routine
interrupt [USART_TXC] void usart_tx_isr(void)
{
if (tx_counter)
   {
   --tx_counter;
   UDR=tx_buffer[tx_rd_index];
   if (++tx_rd_index == TX_BUFFER_SIZE) tx_rd_index=0;
   };
}

#ifndef _DEBUG_TERMINAL_IO_
// Write a character to the USART Transmitter buffer
#define _ALTERNATE_PUTCHAR_
#pragma used+
void putchar(char c)
{
while (tx_counter == TX_BUFFER_SIZE);
#asm("cli")
if (tx_counter || ((UCSRA & DATA_REGISTER_EMPTY)==0))
   {
   tx_buffer[tx_wr_index]=c;
   if (++tx_wr_index == TX_BUFFER_SIZE) tx_wr_index=0;
   ++tx_counter;
   }
else
   UDR=c;
#asm("sei")
}
#pragma used-
#endif

// Standard Input/Output functions
#include <stdio.h>


// SPI functions
#include <spi.h>

// Declare your global variables here
void pc_comm(void)
{          
 unsigned char tmp;
 unsigned int i;
 TIMSK=0x0;
 getchar();
 getchar();
 tmp=getchar();
 if(tmp!=0x00)
  while(1);
 while(1)
 {
  do{
  #asm("wdr")
  tmp=getchar();
  }while(tmp==0x00);
  if(tmp!=0xC3)
   while(1);
  #asm("wdr")
 tmp=getchar();
 if(tmp!=0x00)
 {
  switch (tmp) {
    case 0x84: {          
              LED_ON  
              _CS=0;
              spi(0x84);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              for(i=0;i<256;i++)
               spi(getchar());
              _CS=1;
              }
    break;
    case 0x87: {  
              LED_ON  
              _CS=0;
              spi(0x87);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              for(i=0;i<256;i++)
               spi(getchar());
              _CS=1;
              }
    break;
    case 0x83:{     
              LED_OFF  
              _CS=0;
              spi(0x83);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }
    break;
    case 0x86:{      
              LED_OFF  
              _CS=0;
              spi(0x86);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }
    break;
    case 0x58:{
              _CS=0;
              spi(0x58);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }        
    
    break;
    case 0x59:{
              _CS=0;
              spi(0x59);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }
    break;
    case 0x82:{
              _CS=0;
              spi(0x82);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }
    break;
    case 0x85:{
              _CS=0;
              spi(0x85);
              spi(getchar());
              spi(getchar());
              spi(getchar());
              _CS=1;
              }
    break;
    case 0xAA:
    break;
    };  
   }   
  }
}        

void main(void)
{
// Declare your local variables here
unsigned char i;
// Input/Output Ports initialization
// Port A initialization
// Func7=In Func6=In Func5=In Func4=In Func3=In Func2=In Func1=In Func0=In 
// State7=T State6=T State5=T State4=T State3=P State2=P State1=P State0=P 
PORTA=0x0F;
DDRA=0x00;

// Port B initialization
// Func7=Out Func6=In Func5=Out Func4=Out Func3=In Func2=In Func1=In Func0=In 
// State7=0 State6=T State5=0 State4=0 State3=T State2=P State1=T State0=T 
PORTB=0x00;
DDRB=0xB2;

// Port C initialization
// Func7=Out Func6=Out Func5=Out Func4=Out Func3=Out Func2=In Func1=Out Func0=Out 
// State7=1 State6=1 State5=1 State4=1 State3=1 State2=T State1=0 State0=0 
PORTC=0xF8;
DDRC=0xFB;

// Port D initialization
// Func7=In Func6=Out Func5=Out Func4=Out Func3=Out Func2=In Func1=In Func0=In 
// State7=P State6=1 State5=1 State4=0 State3=0 State2=P State1=T State0=T 
PORTD=0xF4;
DDRD=0x78;

// Timer/Counter 0 initialization
// Clock source: System Clock
// Clock value: 172,800 kHz
// Mode: CTC top=OCR0
// OC0 output: Disconnected
TCCR0=0x0D;
TCNT0=0x00;
OCR0=0xDF;

// Timer/Counter 1 initialization
// Clock source: System Clock
// Clock value: 11059,200 kHz
// Mode: Fast PWM top=03FFh
// OC1A output: Non-Inv.
// OC1B output: Discon.
// Noise Canceler: Off
// Input Capture on Falling Edge
// Timer 1 Overflow Interrupt: On
// Input Capture Interrupt: Off
// Compare A Match Interrupt: Off
// Compare B Match Interrupt: Off
TCCR1A=0x83;
TCCR1B=0x09;
TCNT1H=0x00;
TCNT1L=0x00;
ICR1H=0x00;
ICR1L=0x00;
OCR1AH=0x00;
OCR1AL=0x00;
OCR1BH=0x00;
OCR1BL=0x00;

// Timer/Counter 2 initialization
// Clock source: System Clock
// Clock value: 172,800 kHz
// Mode: Normal top=FFh
// OC2 output: Disconnected
ASSR=0x00;
TCCR2=0x04;
TCNT2=0x00;
OCR2=0x00;

// External Interrupt(s) initialization
// INT0: On
// INT0 Mode: Falling Edge
// INT1: On
// INT1 Mode: Rising Edge
// INT2: On
// INT2 Mode: Rising Edge
GICR=0x00;
MCUCR=0x0;
MCUCSR=0x0;
GIFR=0x00;

// Timer(s)/Counter(s) Interrupt(s) initialization
TIMSK=0x00;

// USART initialization
// Communication Parameters: 8 Data, 1 Stop, No Parity
// USART Receiver: On
// USART Transmitter: On
// USART Mode: Asynchronous
// USART Baud rate: 115200
UCSRA=0x00;
UCSRB=0xD8;
UCSRC=0x86;
UBRRH=0x00;
UBRRL=0x05;

// Analog Comparator initialization
// Analog Comparator: Off
// Analog Comparator Input Capture by Timer/Counter 1: Off
ACSR=0x80;
SFIOR=0x00;

// SPI initialization
// SPI Type: Master
// SPI Clock Rate: 2*2764,800 kHz
// SPI Clock Phase: Cycle Start
// SPI Clock Polarity: High
// SPI Data Order: MSB First
SPCR=0x5C;
SPSR=0x01;

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

delay_ms(20);
AT45DB081_init();
delay_ms(20);
LED2_ON
while (1)
      {
      // Place your code here                           
      #asm("wdr")
      if(rx_counter) 
      {
        LED1_ON 
        pc_comm();
      }
};
}                                       