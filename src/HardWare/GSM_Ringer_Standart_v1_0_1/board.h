/*
  CodeVisionAVR C Compiler
  (C) 2009 Taras Drozdovsky, My.
*/

#ifndef _BOARD_INCLUDED_
#define _BOARD_INCLUDED_

#include <mega32.h>

#define CRCEH       1023 
#define CRCEL       1022 
#define HND         1008 

#define LED_ON      PORTC.3=0;
#define LED_OFF     PORTC.3=1;
#define LED_TOD     PORTC.3=~PORTC.2;

#define LED1_ON     PORTC.4=0;
#define LED1_OFF    PORTC.4=1;
#define LED1_TOD    PORTC.4=~PORTC.4;

#define LED2_ON     PORTC.5=0;
#define LED2_OFF    PORTC.5=1;
#define LED2_TOD    PORTC.5=~PORTC.5;

#define LED3_ON     PORTC.6=0;
#define LED3_OFF    PORTC.6=1;
#define LED3_TOD    PORTC.6=~PORTC.6;

#define LED4_ON     PORTC.7=0;
#define LED4_OFF    PORTC.7=1;
#define LED4_TOD    PORTC.7=~PORTC.7;

#define H_SIM_ON    PORTD.4=1;
#define H_SIM_OFF   PORTD.4=0;

// Declare SoftWare Interrupt

#define SI_GEN      PORTD.3=1;
#define SI_RST      PORTD.3=0;

#define SI_UPBW     (1<<1) 
#define SI_MENU     (1<<2)
#define SI_PLAY_LIST (1<<3)

#define BUTT_TYPE   0x04
#define DTMF_TYPE   0x05

// Declare PIN POWER SIM

#define S_RING      (1<<0)
#define S_SMS       (1<<1)
#define S_CRDY      (1<<2)
#define S_POWERDOWN (1<<3)
#define S_ERROR     (1<<4) 
#define S_CREG      (1<<5)
#define S_CREGNOT   (1<<6)

#define SIM_ON    PORTB.1
#define VDD_EXT   PINB.3
      
#define BUT       (PIND.7)
#define NUMA_NUMBER 5

#define _CS PORTB.4

#define DPORT_CS DDRB.0=4;
#define SCK 7
#define MOSI 5
#define MISO 6

//  define PWM
#define OCREG           OCR1AL
#define NUMMELODY       37          

#define RELAY1_ON       PORTC.0=1;                      // relay-1 on
#define RELAY1_OFF      PORTC.0=0;                      // relay-1 off
#define OPENDRAIN1_OFF  PORTC.1=0;
#define OPENDRAIN1_ON   PORTC.1=1;

#define CS PORTB._CS                                      
      
#define rx_buffer_overflowGSM rx_buffer_overflow
#define rx_counterGSM  rx_counter
#define tx_counterGSM  tx_counter
#define putcharGSM     putchar
#define getcharGSM     getchar

#define rx_buffer_overflowPC rx_buffer_overflow1
#define rx_counterPC  rx_counter1
#define tx_counterPC  tx_counter1
#define putcharPC     putchar1
#define getcharPC     getchar1

// Possible respons after executant command
#define C_OK              0             
#define C_ERROR           2
#define C_REG_NET         8
#define C_NOT_REG_NET     9
    
#endif         