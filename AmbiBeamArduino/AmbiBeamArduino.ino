#include "FastLED.h"

// How many leds in your strip?
#define NUM_LEDS 440
// Where is it connected ?
#define DATA_PIN 3

#define LED 13


#define SOH 0x01
#define STX 0x02
#define ETX 0x03
#define ACK 0x06

// Define the array of leds
CRGB leds[NUM_LEDS];

void setup() { 
      Serial.begin(115200);           
      FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
      //Serial.println(F("Ready"));
}

unsigned char state = 0;
uint16_t len = 0;
unsigned char brightness = 254;
uint16_t chunkPos = 0;
unsigned long  timeout;

void loop() { 
  char c = 0;
  
  if(Serial.available())
    switch(state){
      case 0:
        timeout = millis();
        c = Serial.read();
        if ( c == SOH)
        {
          state++;

          //Serial.println(F("SOH"));
        }
        break;
      case 1:
        Serial.readBytes((char*)&len, 2); 
        
        state++;

        //Serial.print(F("len: "));
        //Serial.println(len, DEC);
        break;
      case 2:
        brightness = Serial.read();
        FastLED.setBrightness(brightness);
        
        if (len > 0)
          state++;
        else 
          state = 5;
          
        //Serial.print(F("brightness: "));
        //Serial.println(brightness, DEC);
        break;

      case 3:
        c = Serial.read();
        if(c == STX)
        {
          timeout = millis();
          chunkPos = 0;
          //FastLED.clear();
          state++;
          //Serial.println(F("STX"));
        }
        break;

      case 4:
        timeout = millis();
        chunkPos += Serial.readBytes((char*)leds, len * 3 - chunkPos);
        if (chunkPos >= (len * 3))
        {
          state++;
        }
        //Serial.print(F("chunksize:"));
        //Serial.println(chunkPos, DEC);
        
        break;
        
      case 5:
        c = Serial.read();
        if(c == ETX)
        {
          FastLED.show();      
          state = 0;
          //Serial.println(F("ETX"));
          Serial.write(ACK);
          break;
        }   
    }
    if(millis() - timeout > 250)
    {
      //Serial.println(F("Timeout"));
      state = 0;
    }
}
