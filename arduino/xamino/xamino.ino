#include <AltSoftSerial.h>
#include <SoftwareSerial.h>
#include <WiFiEsp.h>
#include <WiFiEspClient.h>
#include <WiFiEspServer.h>
#include <CmdBuffer.hpp>
#include <CmdCallback.hpp>
#include <CmdParser.hpp>

#define _TASK_SLEEP_ON_IDLE_RUN
#define _TASK_PRIORITY
#define _TASK_WDT_IDS
#define _TASK_TIMECRITICAL
#include <TaskScheduler.h>
#include <TaskSchedulerDeclarations.h>

int ledStatus = LOW;
bool stream = false;
boolean clientAlreadyConnected = false;

#pragma region WiFi

// pin 2 Uno RX --> TX ESP01
// pin 3 Uno TX --> RX ESP01
//SoftwareSerial wifiSerial(2, 3);
AltSoftSerial wifiSerial(8, 9);

char ssid[] = "Xamino";		 // your network SSID (name)
char pass[] = "m1l4m1l4";	// your network password
int status = WL_IDLE_STATUS; // the Wifi radio's status

WiFiEspServer server(80);
WiFiEspClient client;

#pragma endregion

#pragma region Commands

CmdBuffer<100> cmdBuffer;
CmdParser cmdParser;
CmdCallback<6> cmdCallback;

#pragma endregion

#pragma region Tasks

Scheduler r;
Scheduler hpr;

void listenForCommandsTask();
Task listenTask(200, TASK_FOREVER, &listenForCommandsTask, &hpr, true);

void execStreamTask();
Task streamTask(2000, TASK_FOREVER, &execStreamTask, &r);

#pragma endregion

void setup()
{
	#pragma region Control

	pinMode(LED_BUILTIN, OUTPUT);
  	digitalWrite(LED_BUILTIN, LOW);

	Serial.begin(115200);
	wifiSerial.begin(9600);

	#pragma endregion

	#pragma region WiFi

	WiFi.init(&wifiSerial);

	Serial.print("Attempting to start AP ");
	Serial.println(ssid);

	// uncomment these two lines if you want to set the IP address of the AP
	//IPAddress localIp(192, 168, 4, 1);
	//WiFi.configAP(localIp);

	// start access point
	status = WiFi.beginAP(ssid, 10, pass, ENC_TYPE_WPA2_PSK, true);

	Serial.println("Access point started");
	printWifiStatus();

	// start the web server on port 80
	server.begin();
	Serial.println("Server started");

	#pragma endregion

	#pragma region Commands

	cmdCallback.addCmd("HELLO", &helloCmd);
  	cmdCallback.addCmd("ON", &ledOnCmd);
  	cmdCallback.addCmd("OFF", &ledOffCmd);
	cmdCallback.addCmd("START", &startStreamCmd);
	cmdCallback.addCmd("STOP", &stopStreamCmd);
	cmdCallback.addCmd("DISC", &disconnectCmd);

	#pragma endregion

	#pragma region Tasks

	//r.init();
	//hpr.addTask(listenTask);
	//r.addTask(streamTask);
	r.setHighPriorityScheduler(&hpr);
	listenTask.setId(1);
  	streamTask.setId(6); 
	
	//delay(1000);
	
	listenTask.enable();
	//r.enableAll(false);

	#pragma endregion
}

void loop()
{
	r.execute();
}

#pragma region Commands

void helloCmd(CmdParser *myParser)
{
	client.println("CIAO!");
}

void ledOnCmd(CmdParser *myParser)
{
	digitalWrite(LED_BUILTIN, HIGH);  
	ledStatus = HIGH;
	client.println("Led is ON");
}

void ledOffCmd(CmdParser *myParser)
{
  	digitalWrite(LED_BUILTIN, LOW);
  	ledStatus = LOW;
  	client.println("Led is OFF");
}

void startStreamCmd(CmdParser *myParser)
{
	stream = true;
	streamTask.enable();
	//client.println("Start streaming...");
}

void stopStreamCmd(CmdParser *myParser)
{
	stream = false;
	streamTask.disable();
	//client.println("Stop streaming...");
}

void disconnectCmd(CmdParser *myParser)
{
	stream = false;
	clientAlreadyConnected = false;
	streamTask.disable();
	client.println("Disconnecting...");
	client.stop();
}

#pragma endregion

#pragma region Tasks

void listenForCommandsTask(){
	
	if(!stream)
	{
		client = server.available();
	}

	// listen for incoming clients
	// check if the client is connected
	// a client is considered connected if there is still unread data
	if (client && client.connected() && client.available() > 0)
	{
		//clientAlreadyConnected = true;
		//cmdCallback.loopCmdProcessing(&cmdParser, &cmdBuffer, &wifiSerial);
		if(cmdBuffer.readFromSerial(&wifiSerial, 2000))
		{
			if (cmdParser.parseCmd(&cmdBuffer) != CMDPARSER_ERROR) {

				#pragma region Debug
				
				//int av = client.available();
				//Serial.println(av);

				// Serial.print("Line have readed: ");
				// Serial.println(cmdBuffer.getStringFromBuffer());

				// Serial.print("Command: ");
				// Serial.println(cmdParser.getCommand());

				// Serial.print("Size of parameter: ");
				// Serial.println(cmdParser.getParamCount());

				// const size_t count = cmdParser.getParamCount();
				// for (size_t i = 0; i < count; i++) {

				// 	Serial.print("Param ");
				// 	Serial.print(i);
				// 	Serial.print(": ");
				// 	Serial.println(cmdParser.getCmdParam(i));
				// }

				// if (cmdParser.equalCommand_P(PSTR("Quit"))) {
				// 	Serial.println("You have write a QUIT Command!");
				// }

				#pragma endregion

				client.flush();
				cmdCallback.processCmd(&cmdParser);
			} 
			else 
			{
				client.flush();
				Serial.println("Parser error!");
				client.println("Unrecognized Command!");
			}
		}
	}	
}

void execStreamTask()
{
	if(!stream)
		streamTask.disable();

	if(client.connected())
	{
		client.println(random(1,100));		
	}
	else
	{
		Serial.println("Stream Task client disconnected!");
	}
}

#pragma endregion

#pragma region Methods

void printWifiStatus()
{
	IPAddress ip = WiFi.localIP();
	Serial.print("IP Address: ");
	Serial.println(ip);
	Serial.println();
	Serial.print("SSID: ");
	Serial.print(ssid);
	Serial.println();
}

#pragma endregion

