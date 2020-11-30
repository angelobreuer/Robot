#ifndef MAIN_h
#define MAIN_h
#pragma once

#include <SerialLogger.h>
#include <ServerConnection.h>
#include <ESP8266WiFi.h>
#include <Config.h>
#include <IdleDetector.h>
#include <Status.h>

void setup();
void loop();
void tick();

#endif // MAIN_h