/**
 * @file OTA-mDNS-SPIFFS.ino
 * 
 * @author Pascal Gollor (http://www.pgollor.de/cms/)
 * @date 2015-09-18
 * 
 * changelog:
 * 2015-10-22: 
 * - Use new ArduinoOTA library.
 * - loadConfig function can handle different line endings
 * - remove mDNS studd. ArduinoOTA handle it.
 * 
 */

// includes
#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <WiFiUdp.h>
#include <FS.h>
#include <ArduinoOTA.h>
#include <FastLED.h>


/**
 * @brief mDNS and OTA Constants
 * @{
 */
#define HOSTNAME "AMBIBEAM" ///< Hostename. The setup function adds the Chip ID at the end.
/// @}

// How many leds in your strip?
#define NUM_LEDS 440
// Where is it connected ?
#define DATA_PIN 5

// Define the array of leds
CRGB leds[NUM_LEDS];

#define localPort 2222
#define SOH 0x01
#define STX 0x02
#define ETX 0x03
#define ACK 0x06

WiFiUDP Udp;
unsigned char state = 0;
uint16_t len = 0;
unsigned char brightness = 254;
uint16_t chunkPos = 0;
unsigned long  timeout;

char packetBuffer[1330];



/**
 * @brief Default WiFi connection information.
 * @{
 */
const char* ap_default_ssid = "ambibeam"; ///< Default SSID.
const char* ap_default_psk = "esp8266esp8266"; ///< Default PSK.
/// @}

/// Uncomment the next line for verbose output over UART.
//#define SERIAL_VERBOSE

/**
 * @brief Read WiFi connection information from file system.
 * @param ssid String pointer for storing SSID.
 * @param pass String pointer for storing PSK.
 * @return True or False.
 * 
 * The config file have to containt the WiFi SSID in the first line
 * and the WiFi PSK in the second line.
 * Line seperator can be \r\n (CR LF) \r or \n.
 */
bool loadConfig(String *ssid, String *pass)
{
  // open file for reading.
  File configFile = SPIFFS.open("/cl_conf.txt", "r");
  if (!configFile)
  {
    Serial.println("Failed to open cl_conf.txt.");

    return false;
  }

  // Read content from config file.
  String content = configFile.readString();
  configFile.close();
  
  content.trim();

  // Check if ther is a second line available.
  int8_t pos = content.indexOf("\r\n");
  uint8_t le = 2;
  // check for linux and mac line ending.
  if (pos == -1)
  {
    le = 1;
    pos = content.indexOf("\n");
    if (pos == -1)
    {
      pos = content.indexOf("\r");
    }
  }

  // If there is no second line: Some information is missing.
  if (pos == -1)
  {
    Serial.println("Invalid content.");
    Serial.println(content);

    return false;
  }

  // Store SSID and PSK into string vars.
  *ssid = content.substring(0, pos);
  *pass = content.substring(pos + le);

  ssid->trim();
  pass->trim();

#ifdef SERIAL_VERBOSE
  Serial.println("----- file content -----");
  Serial.println(content);
  Serial.println("----- file content -----");
  Serial.println("ssid: " + *ssid);
  Serial.println("psk:  " + *pass);
#endif

  return true;
} // loadConfig


/**
 * @brief Save WiFi SSID and PSK to configuration file.
 * @param ssid SSID as string pointer.
 * @param pass PSK as string pointer,
 * @return True or False.
 */
bool saveConfig(String *ssid, String *pass)
{
  // Open config file for writing.
  File configFile = SPIFFS.open("/cl_conf.txt", "w");
  if (!configFile)
  {
    Serial.println("Failed to open cl_conf.txt for writing");

    return false;
  }

  // Save SSID and PSK.
  configFile.println(*ssid);
  configFile.println(*pass);

  configFile.close();
  
  return true;
} // saveConfig


void showState(unsigned int led, struct CRGB color)
{
  leds[led] = color;
  FastLED.show();
}

/**
 * @brief Arduino setup function.
 */
void setup()
{
  String station_ssid = "";
  String station_psk = "";
  
  FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
  Serial.begin(115200);
  
  delay(100);

  Serial.println("\r\n");
  Serial.print("Chip ID: 0x");
  Serial.println(ESP.getChipId(), HEX);

  // Set Hostname.
  String hostname(HOSTNAME);
  hostname += String(ESP.getChipId(), HEX);
  WiFi.hostname(hostname);

  // Print hostname.
  Serial.println("Hostname: " + hostname);
  //Serial.println(WiFi.hostname());


  // Initialize file system.
  if (!SPIFFS.begin())
  {
    Serial.println("Failed to mount file system");
    showState(0, CRGB::Red);
    return;
  }
  showState(0, CRGB::Green);
  
  // Load wifi connection information.
  if (! loadConfig(&station_ssid, &station_psk))
  {
    station_ssid = "";
    station_psk = "";

    Serial.println("No WiFi connection information available.");
    showState(1, CRGB::Red);
  }
  showState(1, CRGB::Green);
  
  // Check WiFi connection
  // ... check mode
  if (WiFi.getMode() != WIFI_STA)
  {
    WiFi.mode(WIFI_STA);
    delay(10);
  }

  // ... Compare file config with sdk config.
  if (WiFi.SSID() != station_ssid || WiFi.psk() != station_psk)
  {
    Serial.println("WiFi config changed.");

    // ... Try to connect to WiFi station.
    WiFi.begin(station_ssid.c_str(), station_psk.c_str());

    // ... Pritn new SSID
    Serial.print("new SSID: ");
    Serial.println(WiFi.SSID());

    // ... Uncomment this for debugging output.
    //WiFi.printDiag(Serial);
  }
  else
  {
    // ... Begin with sdk config.
    WiFi.begin();
  }

  Serial.println("Wait for WiFi connection.");

  // ... Give ESP 10 seconds to connect to station.
  unsigned long startTime = millis();
  while (WiFi.status() != WL_CONNECTED && millis() - startTime < 10000)
  {
    Serial.write('.');
    //Serial.print(WiFi.status());
    delay(500);
  }
  Serial.println();

  // Check connection
  if(WiFi.status() == WL_CONNECTED)
  {
    // ... print IP Address
    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());
    showState(3, CRGB::Green);
  }
  else
  {
    Serial.println("Can not connect to WiFi station. Go into AP mode.");
    showState(3, CRGB::Blue);
    // Go into software AP mode.
    WiFi.mode(WIFI_AP);

    delay(10);

    WiFi.softAP(ap_default_ssid, ap_default_psk);

    Serial.print("IP address: ");
    Serial.println(WiFi.softAPIP());
  }

  Udp.begin(localPort);

  ArduinoOTA.onStart([]() {
    Serial.println("Start");
  });
  ArduinoOTA.onEnd([]() {
    Serial.println("\nEnd");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
    else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
    else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
    else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
    else if (error == OTA_END_ERROR) Serial.println("End Failed");
  });

  // Start OTA server.
  ArduinoOTA.setHostname((const char *)hostname.c_str());
  ArduinoOTA.begin();

  MDNS.addService("ambibeam", "udp", localPort);
}


/**
 * @brief Arduino loop function.
 */
void loop()
{
  // Handle OTA server.
  ArduinoOTA.handle();
  yield();

  int packetSize = Udp.parsePacket();
  if (packetSize) {
    Serial.print("Received packet of size ");
    Serial.println(packetSize);
    Serial.print("From ");
    IPAddress remoteIp = Udp.remoteIP();
    Serial.print(remoteIp);
    Serial.print(", port ");
    Serial.println(Udp.remotePort());
    
    // read the packet into packetBufffer
    uint16_t packetLen = Udp.read(packetBuffer, 1330);
    if (packetLen > 0) {
      packetBuffer[packetLen] = 0;
  
      // Serial.println("Contents:");
      // Serial.println(packetBuffer);
      char* p = &packetBuffer[0];
      switch(state){
          case 0: {
            timeout = millis();
            if(*(p++) == SOH)
            {
              len = (uint16_t) *(p++) << 8 | *(p++);
            //  Serial.print(F("len: "));
            //  Serial.println(len, DEC);
              
              brightness = *(p++);
            //  Serial.print(F("brightness: "));
            //  Serial.println(brightness, DEC);
              if(*(p++) == STX){
              //  Serial.println(F("got stx"));
                FastLED.setBrightness(brightness);
                chunkPos = 0;
                state++;
              } else { break; }
            } else { break; } 
          }
        case 1: {
          timeout = millis();
          int bytesToCopy = len * 3;
          //Serial.print("bytesToCopy");
          //Serial.println(bytesToCopy, DEC);
          char* pleds = (char*)leds;
                    
          memcpy ( pleds, p, bytesToCopy);

          if (chunkPos + bytesToCopy >= (len * 3))
          {
            //Serial.println("Copy Done");
            //Serial.println("show");
            FastLED.show();      
            Serial.print(F("FPS:"));
            Serial.println(FastLED.getFPS(),DEC);
            state = 0;
          } else {
            chunkPos += bytesToCopy;
          }
          break;
        }
      }
    }
  }
  if(millis() - timeout > 250)
    {
      //Serial.println(F("Timeout"));
      state = 0;
    }
  
}

