# Fruit Keyboard
This is a Windows 10 IoT Core project using the [Adafruit MPR121 Capacitive Touch Sensor](https://www.adafruit.com/product/1982) and a Raspberry Pi. 

Connect different kinds of fruit to 12 Capacitive Touch Sensor and use it as a keyboard.

To easy the development use the [
Windows.IoT.Core.HWInterfaces](https://github.com/mohankrr/Windows.IoT.Core.HWInterfaces) NuGet package to interact with the MPR121 sensor. 

## How to connect the wires
| Raspberry Pi | MPR121 |
|--------------|--------|
| 3.3V| VIN |
| GND | GND |
| SCL | SCL |
| SDA | SDA |
| GPO Pin #5 | IRQ |