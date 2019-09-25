/*
  Analog input,  serial output

  Reads analog input pins, maps the result to a range from 0 to 99.
  Also prints the results to the Serial Monitor.

  created 16 Sep. 2019
  by Jiong Shi
*/
#include "DHT.h"
#include <String.h>

//Definitions
#define MQ2Pin A0 //Analog input 0 of your arduino
#define LightPin A1 //Analog input 1 of your arduino
#define PirPIN 3
#define DHTPIN 2     // Digital pin connected to the DHT sensor
#define DHTTYPE DHT11   // DHT 11
DHT dht(DHTPIN, DHTTYPE);

//varibles
String gy;
int MQ2sensorValue;
int LightsensorValue;
int PirValue;

void setup()
{
  Serial.begin(9600);
  dht.begin();
  pinMode(PirPIN, INPUT);
}

void loop() {
  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
  int h = dht.readHumidity();
  // Read temperature as Celsius (the default)
  int t = dht.readTemperature();
  // Check if any reads failed and exit early (to try again).
  if (isnan(h) || isnan(t)) {
    Serial.println(F("Failed to read from DHT sensor!"));
    return;
  }

  //read MQ2 value
  MQ2sensorValue = map(analogRead(MQ2Pin),0,1023,0,99);
  
  //read light value
  LightsensorValue = map(analogRead(LightPin),0,1023,0,99);
  
  //read PIR value
  if (digitalRead(PirPIN) == HIGH)
    PirValue = 1;
  else
    PirValue = 0;

  //make sure that the send data have the same length
  if(LightsensorValue<10)
    gy = String("") + "A2" + t + h + "0"+LightsensorValue;
    else
    gy = String("") + "A2" + t + h +LightsensorValue;
  if(MQ2sensorValue<10)
    gy += String("") +"0"+MQ2sensorValue+PirValue;
    else
    gy += String("") +MQ2sensorValue+PirValue;   
  Serial.print(gy);

  // Wait a few seconds between measurements.
  delay(5000);
}
