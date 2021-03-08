#include <Bounce2.h>

#include <Adafruit_NeoPixel.h>

#define LED_PIN        2
#define MIC_PIN        4
#define HAND_PIN       5
#define CAM_PIN        6
#define NUMPIXELS 16

#define MIC_BRIGHTNESS      180
#define HAND_BRIGHTNESS      180
#define CAM_BRIGHTNESS      100

#define SHORT_PRESS_TIME 500
#define LONG_PRESS_TIME  1000

// Variables will change:
int lastMicState = LOW;  // the previous state from the input pin
int currentMicState;     // the current reading from the input pin
unsigned long pressedMicTime  = 0;
unsigned long releasedMicTime = 0;
bool isMicLightOn = false;
bool isMicPressed = false;

// Variables will change:
int lastHandState = LOW;  // the previous state from the input pin
int currentHandState;     // the current reading from the input pin
unsigned long pressedHandTime  = 0;
unsigned long releasedHandTime = 0;
bool isHandLightOn = false;
bool isHandPressed = false;

// Variables will change:
int lastCamState = LOW;  // the previous state from the input pin
int currentCamState;     // the current reading from the input pin
unsigned long pressedCamTime  = 0;
unsigned long releasedCamTime = 0;
bool isCamLightOn = false;
bool isCamPressed = false;
bool isInCall = false;
Adafruit_NeoPixel pixels(NUMPIXELS, LED_PIN, NEO_GRB + NEO_KHZ800);

// INSTANTIATE A Button OBJECT
Bounce2::Button micButton = Bounce2::Button();
Bounce2::Button handButton = Bounce2::Button();
Bounce2::Button camButton = Bounce2::Button();

void setup() {
  Serial.begin(9600);
  // put your setup code here, to run once:
  pixels.begin();

  micButton.attach(MIC_PIN, INPUT_PULLUP);
  micButton.interval(50);
  micButton.setPressedState(LOW);

  handButton.attach(HAND_PIN, INPUT_PULLUP);
  handButton.interval(50);
  handButton.setPressedState(LOW);

  camButton.attach(CAM_PIN, INPUT_PULLUP);
  camButton.interval(50);
  camButton.setPressedState(LOW);

  pixels.clear();
  pixels.show();

}

void loop() {
  micButton.update();
  handButton.update();
  camButton.update();

  if (Serial.available() > 0) {
    // read the incoming byte:
    String line = Serial.readString();
    line.trim();
    if (line.substring(0) == "keepAlive") {}
    else if (line.substring(0) == "call" || line.substring(0) == "presenting") {
      isCamLightOn = true;
      isHandLightOn = false;
      isMicLightOn = true;
      setLedColor(MIC_BRIGHTNESS, 0, 0);
      isInCall = true;
    }
    else {
      isInCall = false;
      isCamLightOn = false;
      isHandLightOn = false;
      isMicLightOn = false;
      setLedColor(0, 0, 0);
    }
    // say what you got:
    Serial.println("ack: " + line);
  }

  if (micButton.pressed())
  {
    pressedMicTime = millis();
    isMicPressed = true;
  } else if (micButton.released())
  {
    if (isMicPressed) {
      isMicPressed = false;
      releasedMicTime = millis();

      if ( releasedMicTime - pressedMicTime < SHORT_PRESS_TIME )
      {
        Serial.println("mic");
        isMicLightOn = !isMicLightOn;
      }
    }
  }
  if (isMicPressed) {
    if ( pressedMicTime + LONG_PRESS_TIME < millis() )
    {
      isMicPressed = false;
      isMicLightOn = !isMicLightOn;
    }
  }

  if (handButton.pressed())
  {
    pressedHandTime = millis();
    isHandPressed = true;
  } else if (handButton.released())
  {
    if (isHandPressed) {
      isHandPressed = false;
      releasedHandTime = millis();

      if ( releasedHandTime - pressedHandTime < SHORT_PRESS_TIME )
      {
        Serial.println("hand");
        isHandLightOn = !isHandLightOn;
      }
    }
  }
  if (isHandPressed) {
    if ( pressedHandTime + LONG_PRESS_TIME < millis() )
    {
      isHandPressed = false;
      isHandLightOn = !isHandLightOn;
    }
  }

  if (camButton.pressed())
  {
    pressedCamTime = millis();
    isCamPressed = true;
  } else if (camButton.released())
  {
    if (isCamPressed) {
      isCamPressed = false;
      releasedCamTime = millis();

      if ( releasedCamTime - pressedCamTime < SHORT_PRESS_TIME )
      {
        Serial.println("cam");
        isCamLightOn = !isCamLightOn;
      }
    }
  }

  if (isCamPressed) {
    if ( pressedCamTime + LONG_PRESS_TIME < millis() )
    {
      isCamPressed = false;
      isCamLightOn = !isCamLightOn;
    }
  }

  if (isMicLightOn) {
    setLedColor(MIC_BRIGHTNESS, 0, 0);
  } else if (isHandLightOn) {
    setLedColor(200, 100, 0);
  } else if (isCamLightOn) {
    setLedColor(0, isCamLightOn ? CAM_BRIGHTNESS : 0, 0);
  } else {
    setLedColor(0, 0, 0);
  }

}

void setLedColor(byte r, byte g, byte b) {
  for (int i = 0; i < NUMPIXELS; i++) { // For each pixel...
    // pixels.Color() takes RGB values, from 0,0,0 up to 255,255,255
    // Here we're using a moderately bright green color:
    pixels.setPixelColor(i, pixels.Color(r, g, b));
    // Send the updated pixel colors to the hardware.
  }
  pixels.show();
}
