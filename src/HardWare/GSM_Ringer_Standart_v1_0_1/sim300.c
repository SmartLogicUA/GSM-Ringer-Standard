/*
  CodeVisionAVR C Compiler
  (C) 2007 Taras Drozdovsky, My.
*/
#include <stdio.h>
#include <board.h>

#define ATA         "ATA"        
#define ATH         "ATH0"
#define CMGR        "AT+CMGR=1"
#define ATE0        "ATE0"
#define CFUN        "AT+CFUN=1"
#define VTD         "AT+VTD=2"

//Declare string AT-command             
#define CNMI        "AT+CNMI=2,1,0,0,0"
#define CSMP        "AT+CSMP=17,169,0,0"
#define CMGF        "AT+CMGF=1"
#define CMGS        "AT+CMGS=\"+38"
#define CMGD        "AT+CMGD=1"
#define CMGDA       "AT+CMGDA=\"DEL ALL\""
#define CCLKS       "AT+CCLK=\""
#define CCLKG       "AT+CCLK?"
#define CLCC        "AT+CLCC"
#define CREGG       "AT+CREG?"
#define VTS         "AT"
#define ATDD        "ATD+38"          
#define ATD         "ATD"          
#define CUSD        "AT+CUSD=1"
#define CUSD_       "+CUSD:"
#define CLVL        "AT+CLVL=83"
#define CMIC0       "AT+CMIC=0,15"
#define CMIC1       "AT+CMIC=1,0"
#define AT          "AT"
#define CSCA        "AT+CSCA?"
#define CSCS        "AT+CSCS=\"UCS2\""
#define CMGLU       "AT+CMGL=\"REC UNREAD\""
#define CHFA0       "AT+CHFA=0"
#define CHFA1       "AT+CHFA=1"
// Receive message from telephone 
#define RING        "RING"
#define CMTI        "+CMTI: \"SM\","
#define CMGS_       "+CMGS:"
#define CREG_N1     "+CREG: 1,0"
#define CREG_N2     "+CREG: 1,2"
#define CREG_Y      "+CREG: 1,1"
#define NOCARRIER   "NO CARRIER"
#define NOANSWER    "NO ANSWER"
#define BUSY        "BUSY"
#define OK          "OK"
#define ERROR       "ERROR"
#define CONNECT9600 "CONNECT 9600"
#define SIMREADY    "Call Ready" 
#define CIPSEND     "AT+CIPSEND"
#define CIPCLOSE    "AT+CIPCLOSE"
#define CIPSHUT     "AT+CIPSHUT"

#define ATE0V1      "ATE0V1"
#define CGATT       "AT+CGATT=1"

#define CREG        "AT+CREG=1"
#define CGREG       "AT+CGREG=1"
#define CGREGQ      "AT+CGREG?"
#define CGREG10     "+CGREG: 1,0"
#define CGREG11     "+CGREG: 1,1"
#define CGREG0      "+CGREG: 0"
#define CGREG1      "+CGREG: 1"

#define CONNECTOK   "CONNECT OK"
#define CONNECTFAIL "CONNECT FAIL"
#define SENDOK      "SEND OK"
#define SENDFAIL    "SEND FAIL"
#define SHUTOK      "SHUT OK"


/**
 * On power Sim300 
 **/
void SimPowerOn(void)
{
    #asm("wdr")  
    if(!VDD_EXT)
    {
        SIM_ON=0;
        delay_ms(2000);
        SIM_ON=1;
        CounterGSM=400;     
        while((!VDD_EXT)&&(CounterGSM))
        #asm("wdr");        

    }
}

/**
 * Off power Sim300 
 **/
void SimPowerOff(void)
{
    #asm("wdr")  
    if(VDD_EXT)
    {  
        SIM_ON=0;
        delay_ms(700);
        SIM_ON=1;
        CounterGSM=400;     
        while((VDD_EXT)&&(CounterGSM))
        #asm("wdr");        
    }
    else
    {
        delay_ms(3000);
    } 
}

/**
 * Reset Sim300  
 **/
void SimRst(void)
{           
    #asm("wdr")     
    LED_ON           
    SimPowerOff();
    delay_ms(2000);
    SimPowerOn();
    SendCommand("AT");        
    SendCommand("AT");        
    LED_OFF
}

/**
 * Send command PIML with Flash memory string.  
 *
 * @param	*data	a pointer to the string command 
 **/
void SendCommand(flash unsigned char *data){
 unsigned char i=0;
  do {     
   #asm("wdr")
   putcharGSM(data[i]);
  }while(data[++i]);
  putcharGSM(0x0D);  
}
/**
 * Send text SIM with Flash memory string.  
 *
 * @param	*data	a pointer to the string text 
 **/
void Send_textf(flash char *data){
 unsigned char i=0;
  do {        
   #asm("wdr")
   putcharGSM(data[i]);
  }while(data[++i]);
}
     
/**
 * Hendler events from SIM.  
 *
 * @return	0	event processing
 * @return	1	event not processing
 **/
unsigned char HandlerEventGSM(void)
{             
    #asm("wdr")      
    if((strcmpf(&gsm_rx_buffer[0],RING)==0))       // Receive RING
    {          
        status_sim|=S_RING;
        GSM_PACK=0;  
        return 0;
    }
    if((strstrf(&gsm_rx_buffer[0],CMTI)!=NULL))     // Receive SMS  
    {    
        status_sim|=S_SMS;         
        GSM_PACK=0;
        return 0;     
    }    
    if((strcmpf(&gsm_rx_buffer[0],ERROR)==0))      // Receive ERROR
    {          
        status_sim|=S_ERROR;         
        GSM_PACK=0;
        return 0;
    } 
    if((strcmpf(&gsm_rx_buffer[0],SIMREADY)==0))       // Receive Call Ready
    {
        status_sim|=S_CRDY;              
        GSM_PACK=0;          
        return 0;
    }                        
    if((strcmpf(&gsm_rx_buffer[0],"+CREG: 1")==0))       // Network register
    {
        status_sim|=S_CREG;              
        GSM_PACK=0;          
        return 0;
    }                            
    if((strcmpf(&gsm_rx_buffer[0],"+CREG: 0")==0))       // Not network register
    {
        status_sim|=S_CREGNOT;              
        GSM_PACK=0;          
        return 0;
    }                            
    if((strcmpf(&gsm_rx_buffer[0],"+CREG: 2")==0))       // Not network register
    {
        status_sim|=S_CREGNOT;              
        GSM_PACK=0;          
        return 0;
    }                            
    GSM_PACK=0;
    return 1;
}                         

/**
 * Handler event as "CLCC".  
 *
 * @return	OK	if number register in base 
 **/
unsigned char C_CLCC(void)
{     
    unsigned char i=0,j=0;
    #asm("wdr")
    SendCommand(CLCC);
    while(tx_counterGSM);
    CounterGSM=25;
    S_CLCC.stat=0;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;         
    if(strstrf(&gsm_rx_buffer[0],"+CLCC:")!=NULL)
    {
        while(gsm_rx_buffer[i++]!=',');
        while(gsm_rx_buffer[i++]!=',');
        S_CLCC.stat=gsm_rx_buffer[i];  
        while(gsm_rx_buffer[i++]!='"');
        while(gsm_rx_buffer[i]!='"')
            S_CLCC.number[j++]=gsm_rx_buffer[i++];
        S_CLCC.number[j]=0;
        GSM_PACK=0;
        gsm_rx_counter=0;
        CounterGSM=25;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 1;         
    }              
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        gsm_rx_counter=0;
        return 0;
    }
    return 1;                              
}            

/**
 * hund-up current Call  
 *
 * @return	OK	hand-up Call disconnected
 * @return	ERROR	hand_up ERROR
 **/ 
unsigned char C_ATH(void)
{
    #asm("wdr")
    SendCommand(ATH);
    while(tx_counterGSM);
    CounterGSM=100;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;         
    if ((strcmpf(&gsm_rx_buffer[0],"NO CARRIER")==0))      // No Carrier  
    { 
        GSM_PACK=0;
        CounterGSM=100;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 1;         
    }
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}   

/**
 * AT  
 *
 * @return	OK	
 * @return	ERROR	
 **/ 
unsigned char C_AT(void){
  #asm("wdr")
  GSM_PACK=0;
  SendCommand(AT);
  while(tx_counterGSM);
  CounterGSM=100;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();
  if(!GSM_PACK) return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  return 1;             
} 

/**
 * CHFA0  
 *
 * @return	OK	
 * @return	ERROR	
 **/ 
unsigned char C_CHFA0(void){
  #asm("wdr")
  GSM_PACK=0;
  SendCommand(CHFA0);
  while(tx_counterGSM);
  CounterGSM=100;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();
  if(!GSM_PACK) return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  return 1;             
} 
/**
 * CHFA1  
 *
 * @return	OK	
 * @return	ERROR	
 **/ 
unsigned char C_CHFA1(void){
  #asm("wdr")
  GSM_PACK=0;
  SendCommand(CHFA1);
  while(tx_counterGSM);
  CounterGSM=100;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();
  if(!GSM_PACK) return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  return 1;             
} 
/**
 * Call accepted 
 *
 * @return	OK	Call accepted 
 * @return	ERROR	ERROR
 **/ 
unsigned char C_ATA(void){
  #asm("wdr")
  SendCommand(ATA);
  while(tx_counterGSM);
  CounterGSM=40;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();
  if(!GSM_PACK)
   return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  return 1;             
}    

/**
 * Call accepted 
 *
 * @return	OK	Call accepted 
 * @return	ERROR	ERROR
 **/ 
unsigned char C_ATDD(unsigned char *mas)
{
  unsigned char i;
  #asm("wdr")
  Send_textf(ATDD);
  for(i=0;i!=10;i++)
   putcharGSM(mas[i]);
  putcharGSM(';');   
  putcharGSM(0x0D); 
  while(tx_counterGSM);
  CounterGSM=100;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();
  if(!GSM_PACK)return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  if((strcmpf(&gsm_rx_buffer[0],"NO DIALTONE"))==0)
  {
   GSM_PACK=0;
   return 1; 
  }  
  return 1;             
}    

/**
 * Delete SMS message 
 *
 * @return	OK	delete successfull 
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMGD(void)
{
    #asm("wdr")
    SendCommand(CMGD);
    while(tx_counterGSM);
    CounterGSM=800;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();          
    if(!GSM_PACK)
        return 1;         
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}         

/**
 * Delete all SMS message 
 *
 * @return	OK	delete successfull 
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMGDA(void){ 
  #asm("wdr")
  SendCommand(CMGDA);
  while(tx_counterGSM);
  CounterGSM=800;
  while((!GSM_PACK)&&(CounterGSM))
   Receive_gsm();          
  if(!GSM_PACK)
   return 1;         
  if((strcmpf(&gsm_rx_buffer[0],OK))==0)
  {
   GSM_PACK=0;
   return 0; 
  }  
  return 1;              
}      

/**
 * Delete all SMS message 
 *
 * @return	OK	delete successfull 
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMGLU(unsigned char *mas)
{
    #asm("wdr")
    SendCommand(CMGLU);
    while(tx_counterGSM);
    CounterGSM=1000;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();          
    if(!GSM_PACK)
        return 1;       
    GSM_PACK=0;   
    gsm_rx_counter=0;
    CounterGSM=1000;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();          
    if(!GSM_PACK)
        return 1;         
     if((strcmpf(&gsm_rx_buffer[0],OK))==0)
     { 
         GSM_PACK=0;   
         return 0; 
     }                       
     else
         strcpy(&mas[0],&gsm_rx_buffer[0]);
    return 1; 
}      

/**
 * New message indication  
 *
 * @return	OK	successfull
 * @return	ERROR	ERROR
 **/   
unsigned char C_CNMI(void)
{           
    #asm("wdr")
    SendCommand(CNMI);   
    while(tx_counterGSM);
    CounterGSM=50;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}   

/**
 * Gain Speaker   
 *
 * @return	OK	successfull
 * @return	ERROR	ERROR
 **/   
unsigned char C_CLVL(void)
{           
    #asm("wdr")
    SendCommand(CLVL);   
    while(tx_counterGSM);
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }      
    return 1;              
}   

/**
 * Gain Microphone main   
 *
 * @return	OK	successfull
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMIC0(void)
{           
    #asm("wdr")
    SendCommand(CMIC0);   
    while(tx_counterGSM);
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}

/**
 * Gain Microphone aux   
 *
 * @return	OK	successfull
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMIC1(void)
{           
    #asm("wdr")
    SendCommand(CMIC1);   
    while(tx_counterGSM);
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}

/**
 * Set Text Mode Parameters SMS  
 *
 * @return	OK	success full
 * @return	ERROR	ERROR
 **/   
unsigned char C_CSMP(void)
{           
    #asm("wdr")
    SendCommand(CSMP);
    while(tx_counterGSM);
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}   
                    
/**
 * Set Text Mode Unicod Parameters SMS  
 *
 * @return	OK	success full
 * @return	ERROR	ERROR
 **/   
unsigned char C_CSCS(void)
{           
    #asm("wdr")
    SendCommand(CSCS);
    while(tx_counterGSM);
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}                       
                               
/**
 * Echo mode off  
 *
 * @return	OK	success full
 * @return	ERROR	ERROR
 **/   
unsigned char C_ATE0(void)
{            
    #asm("wdr")
    SendCommand(ATE0);
    gsm_rx_counter=0;
    GSM_PACK=0;
    while(tx_counterGSM);
    CounterGSM=50;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;        
    if((strcmpf(&gsm_rx_buffer[0],ATE0))==0)
    {
        GSM_PACK=0;
        CounterGSM=50;
        gsm_rx_counter=0;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 1;
    }    
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;                  
}           
//    printf("\r\nCounterGSM=%d",CounterGSM);
//    printf("\r\nStringGSM_ATE0=%s",&gsm_rx_buffer[0]);
           
/**
 * Preferred Message Format SMS  
 *
 * @return	OK	success full
 * @return	ERROR	ERROR
 **/   
unsigned char C_CMGF(void)
{            
    #asm("wdr")
    SendCommand(CMGF);
    while(tx_counterGSM);
    CounterGSM=400;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}     
/**
 * Preferred Message Format SMS  
 *
 * @return	OK	success full
 * @return	ERROR	ERROR
 **/   
unsigned char C_VTD(void)
{            
    #asm("wdr")
    SendCommand(VTD);
    while(tx_counterGSM);
    CounterGSM=400;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}     


 /*     
 * Send SMS
 * @param	*massage      a pointer to the string in RAM   
 * @param	*number	a pointer to the string in RAM number phone abonent  
 *
 * @return	OK	massage send
 * @return	ERROR	error
 */   
unsigned char SendSMS(char *massage, char *number)
{
    unsigned char i=0;         
        
    while(tx_counterGSM);
    #asm("wdr")     
    Send_textf(CMGS); 
    do
    {  
        #asm("wdr")
        putcharGSM(number[i]);
    }while(number[++i]);
    putcharGSM('"');
    putcharGSM(0x0D);              
    while(tx_counterGSM);
    CounterGSM=600;
    while((getcharGSM()!='>')&&(CounterGSM))
    #asm("wdr");
    while((getcharGSM()!=' ')&&(CounterGSM))
    #asm("wdr");
    if(!CounterGSM)
        return 1;
    i=0;
    do
    {  
        #asm("wdr")
        putcharGSM(massage[i]);
    }while(massage[++i]);
    putcharGSM(26);
    gsm_rx_counter=0;
    GSM_PACK=0; 
    #asm("cli")
    CounterGSM=2000;
    #asm("sei")
    #asm("wdr")
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strstrf(&gsm_rx_buffer[0],CMGS_)!=NULL))
    {              
        GSM_PACK=0;
        #asm("wdr")
        CounterGSM=500;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 1;
        if((strcmpf(&gsm_rx_buffer[0],OK))==0)
        {
            GSM_PACK=0;
            return 0; 
        }
    }                         
    return 1; 
}      
                                                                            
/*     
 * Check network registration
 *
 * @eturn	C_REG_NET	registered, home network
 * @return	ERROR	error
 */   
unsigned char C_CREGG(void)
{ 
    #asm("wdr")
    SendCommand(CREGG);
    while(tx_counterGSM)
    #asm("wdr");
    CounterGSM=100;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 0;
    if((strcmpf(&gsm_rx_buffer[0],CREG_Y))==0)
    {
        GSM_PACK=0;
        CounterGSM=100;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 0;   
        if((strcmpf(&gsm_rx_buffer[0],OK))==0)
        {
            GSM_PACK=0;
            return C_REG_NET; 
        }   
    } 
    if((strcmpf(&gsm_rx_buffer[0],CREG_N1)==0)||(strcmpf(&gsm_rx_buffer[0],CREG_N2)==0))
    {
        GSM_PACK=0;
        CounterGSM=100;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 0;
        if((strcmpf(&gsm_rx_buffer[0],OK))==0)
        {
            GSM_PACK=0;
            return C_NOT_REG_NET; 
        }
        return 0;  
    }  
    return 0;              
}      
                                             
/*     
 * Receive byte from Sim300 and shaping packet
 */  
void Receive_gsm(void)
{
    unsigned char data;
 
    #asm("wdr")              
    if(rx_counterGSM)
    {
        data=getcharGSM();
        switch (data) {
            case 0x0D: if(gsm_rx_counter){gsm_rx_buffer[gsm_rx_counter]=0;GSM_PACK=1;}
            break;
            case 0x0A: gsm_rx_counter=0;
            break;   
            default:
                {
                    gsm_rx_buffer[gsm_rx_counter++]=data;
                    if(gsm_rx_counter==200)gsm_rx_counter=0;
                }
            };
   } 
}  

/*
 * ATE0V1 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_ATE0V1(void)
{
    #asm("wdr")
    SendCommand(ATE0V1);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}     

/*
 * C_CGREG 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_CGREG(void)
{
    #asm("wdr")
    SendCommand(CGREG);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}   
  
/*
 * C_CREG 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_CREG(void)
{
    #asm("wdr")
    SendCommand(CREG);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}   

/*
 * C_CGREG=? 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_CGREGQ(void)
{
    #asm("wdr")
    SendCommand(CGREGQ);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],CGREG10))==0)
    {      
        GSM_PACK=0;                
        CounterGSM=40;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK) 
            return 1;
        if((strcmpf(&gsm_rx_buffer[0],OK))==0)
        {
            GSM_PACK=0;
            return 0; 
        }  
        GSM_PACK=0;
        return 0; 
    }  
    if((strcmpf(&gsm_rx_buffer[0],CGREG11))==0)
    {          
        GSM_PACK=0;                
        CounterGSM=40;
        while((!GSM_PACK)&&(CounterGSM))
            Receive_gsm();
        if(!GSM_PACK)
            return 1;   
        if((strcmpf(&gsm_rx_buffer[0],OK))==0)
        {
            GSM_PACK=0;
            return 1; 
        }  
        GSM_PACK=0;
        return 0; 
    }  
    if ((strcmpf(&gsm_rx_buffer[0],ERROR)==0))      // Receive ERROR
    {          
        return 0;
    } 
    return 0;             
}   
  
/*
 * C_CFUN 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_CFUN(void)      
{
    #asm("wdr")
    SendCommand(CFUN);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}   
  
/*
 * C_CUSD 
 *
 * @return	OK	 
 * @return	ERROR	ERROR
 */ 
unsigned char C_CUSD(void)      
{
    #asm("wdr")
    SendCommand(CUSD);
    while(tx_counterGSM);
    GSM_PACK=0;                
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;             
}   
              
/**
 * USSD 
 *
 * @return	OK	Call accepted 
 * @return	ERROR	ERROR
 **/ 
unsigned char C_USSD(unsigned char *mas)
{
    unsigned char i=0;
    
    #asm("wdr")
    Send_textf(ATD);
    do
    {     
        #asm("wdr")       
        if(mas[i]=='@') putcharGSM('*');
        else putcharGSM(mas[i]);
    }while(mas[++i]);
    putcharGSM(0x0D);  
    while(tx_counterGSM);
    CounterGSM=2000;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;         
    if((strstrf(&gsm_rx_buffer[0],CUSD_))!=NULL)
    {            
        i=10;
        do
        {     
            #asm("wdr")
            if(gsm_rx_buffer[i]=='*') 
                gsm_rx_buffer[i]='@';
            if(gsm_rx_buffer[i]=='"') 
                gsm_rx_buffer[i+1]=0;
        }while(gsm_rx_buffer[++i]);
        gsm_rx_buffer[3]='S';   
        gsm_rx_buffer[4]='Y';   
        gsm_rx_buffer[5]='U';   
        gsm_rx_buffer[6]='S';   
        gsm_rx_buffer[7]='D';   
        gsm_rx_buffer[8]=' ';   
        SendAnswerR(&gsm_rx_buffer[3]);
        GSM_PACK=0; 
        return 0; 
    }
    SendAnswerF("SYUSD ERROR");  
    return 1;             
}    

 /*     
 * Send tone DTMF sending
 *
 * @param	*mas	a pointer to the string in FLASH with code tone DTMF sending  
 *
 * @return	OK	massage send
 * @return	ERROR	error
 */   
unsigned char C_VTS(flash char *mas)
{ 
    unsigned char i=0; 
    #asm("wdr") 
    Send_textf(VTS);
    do
    {     
        #asm("wdr")
        putcharGSM(mas[i]);
    }while(mas[++i]);
    putcharGSM(0x0D);
    while(tx_counterGSM)
    #asm("wdr");
    CounterGSM=40;
    while((!GSM_PACK)&&(CounterGSM))
        Receive_gsm();
    if(!GSM_PACK)
        return 1;
    if((strcmpf(&gsm_rx_buffer[0],OK))==0)
    {
        GSM_PACK=0;
        return 0; 
    }  
    return 1;              
}   
   
 /*     
 * Send SMS Unicod
 * @param	*massage      a pointer to the string in RAM   
 * @param	*number	a pointer to the string in RAM number phone abonent  
 *
 * @return	OK	massage send
 * @return	ERROR	error
 */   
// unsigned char SendSMSU(flash char *massage, flash char *number)
// {
//  unsigned char i=0;         
//  while(tx_counterGSM);
//  #asm("wdr")     
//  Send_textf(&CMGSU[0]); 
//  do {  
//  #asm("wdr")
//  putcharGSM(number[i]);
//  } while(number[++i]);
//  putcharGSM('"');
//  putcharGSM(0x0D);              
//  while(tx_counterGSM);
//  CounterGSM=2000;
//  while((getcharGSM()!=0x3E)&&(CounterGSM))
//  #asm("wdr");
//  if(!CounterGSM)
//   return 1;
//  i=0;
//  do {  
//      #asm("wdr")
//      putcharGSM(massage[i]);
//     } while(massage[++i]);
//  putcharGSM(26);
//  gsm_rx_counter=0;
//  GSM_PACK=0;
//  CounterGSM=2000;
//  while((!GSM_PACK)&&(CounterGSM))
//   Receive_gsm();
//  if(!GSM_PACK)
//   return 1;
//  gsm_rx_buffer[gsm_rx_counter]=0;
//  if((strstrf(&gsm_rx_buffer[0],&CMGS_[0])!=NULL))
//  {
//   GSM_PACK=0;
//   #asm("wdr")
//   CounterGSM=40;
//   while((!GSM_PACK)&&(CounterGSM))
//    Receive_gsm();
//   if(!GSM_PACK)
//    return 1;
//   if((strcmpf(&gsm_rx_buffer[0],&OK[0]))==0)
//   {
//    GSM_PACK=0;
//    return 0; 
//   }
//  }  
//  return 1; 
//}           