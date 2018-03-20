#include <AltSoftSerial.h>

//PIN2 arduino RX --> TX della schedina ESP01
//PIN3 arduino TX --> RX della schedina ESP01
AltSoftSerial esp8266(8, 9);

void setup() {
  //dipende da come è settata la ESP01
  //provare con entrambe le seriali su 9600
  //oppure provare con entrambe settate a 115200

  //è possibile cambiare la velocità della seriale sulla schedina con il comando
  //vedere la documentazione dei comandi AT per la differenza tra _CUR e _DEF
  //AT+UART=9600,8,1,0,0
  //AT+UART_CUR=9600,8,1,0,0
  //AT+UART_DEF=9600,8,1,0,0
  
  Serial.begin(9600);
  esp8266.begin(9600); 
  
  esp8266.write("AT\r\n");
}

void loop() {
  if (esp8266.available()) {
    Serial.write(esp8266.read());
  }
  if (Serial.available()) {
    esp8266.write(Serial.read());
  }
}
