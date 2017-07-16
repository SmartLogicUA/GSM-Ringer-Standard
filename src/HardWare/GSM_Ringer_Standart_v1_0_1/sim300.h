/*
  CodeVisionAVR C Compiler
  (C) 2007 Taras Drozdovsky, My.
*/

#ifndef _SIM300_INCLUDED_
#define _SIM300_INCLUDED_   

unsigned char gsm_rx_buffer[200];    
unsigned char gsm_rx_counter=0;

unsigned char status_sim=0;

struct {
        unsigned char stat;
        unsigned char number[14];
       }S_CLCC;

bit GSM_0D=0;
bit GSM_0A=0;
bit GSM_PACK=0;


#pragma used+

void SimPowerOn(void);                
void SimPowerOff(void);                
void SimRst(void);  
void Receive_gsm(void);
unsigned char HandlerEventGSM(void);            
void C_CSCA(void);                           
unsigned char C_ATA(void);
unsigned char C_ATH(void);  
unsigned char C_ATDD(unsigned char *);
unsigned char C_CMGD(void);
unsigned char C_CMGDA(void);  
unsigned char C_CMGLU(unsigned char *);
unsigned char C_CNMI(void);           
unsigned char C_CMGF(void);
unsigned char C_CSMP(void);     
unsigned char C_CSCS(void);     
unsigned char C_CLVL(void);     
unsigned char C_CMIC0(void);   
unsigned char C_CMIC1(void);   
unsigned char C_AT(void);  
unsigned char C_CIPCSGP(void);
unsigned char C_CIPSTART(void);   
unsigned char C_CIPCLOSE(void);
unsigned char C_CIPSHUT(void); 
unsigned char SendData(char *);
unsigned char C_ATE0(void);   
unsigned char C_CLCC(void);  
unsigned char C_CFUN(void);
unsigned char C_VTD(void);
unsigned char C_CUSD(void);
unsigned char C_CHFA0(void);
unsigned char C_CHFA1(void);
unsigned char C_USSD(unsigned char *);
unsigned char C_ATE0V1(void);
unsigned char C_CREGG(void);
unsigned char C_CGREG(void);
unsigned char C_CREG(void);
unsigned char C_CGATT(void);
unsigned char C_CIPSTATUS(void);
unsigned char C_CGREGQ(void);
unsigned char C_VTS(flash unsigned char *);
void SendCommand(flash unsigned char *);
void Send_textf(flash char *);
unsigned char SendSMS(char *,char *);
unsigned char SendSMSU(flash char *,flash char *);
unsigned char TransmitGPRSData(void);
//unsigned char C_CREGG(void);  
unsigned char SetTime(char *);
unsigned char GetTime(unsigned char *mas);           

#pragma used-
#endif
