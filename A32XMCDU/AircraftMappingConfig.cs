using System.Collections.Generic;

namespace A32XMCDU
{
    public class LedBinding
    {
        public string DataRef { get; set; }
        public float Condition { get; set; }
        public int LedPin { get; set; }
    }

    public static class AircraftMappingConfig
    {
        public static readonly Dictionary<string, Dictionary<string, string>> Mappings = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "Toliss A320 family", new Dictionary<string, string>()
                {
                    // Brightness
                    { "R8C4", "DREF_UP:AirbusFBW/DUBrightness[{bright}]" },
                    { "R8C5", "DREF_DN:AirbusFBW/DUBrightness[{bright}]" },

                    { "R3C1", "AirbusFBW/MCDU{side}LSK1L" },
                    { "R3C2", "AirbusFBW/MCDU{side}LSK2L" },
                    { "R3C3", "AirbusFBW/MCDU{side}LSK3L" },
                    { "R1C1", "AirbusFBW/MCDU{side}LSK4L" },
                    { "R1C2", "AirbusFBW/MCDU{side}LSK5L" },
                    { "R1C3", "AirbusFBW/MCDU{side}LSK6L" },

                    { "R7C1", "AirbusFBW/MCDU{side}LSK1R" },
                    { "R7C2", "AirbusFBW/MCDU{side}LSK2R" },
                    { "R7C3", "AirbusFBW/MCDU{side}LSK3R" },
                    { "R8C1", "AirbusFBW/MCDU{side}LSK4R" },
                    { "R8C2", "AirbusFBW/MCDU{side}LSK5R" },
                    { "R8C3", "AirbusFBW/MCDU{side}LSK6R" },

                    { "R1C4", "AirbusFBW/MCDU{side}DirTo" },
                    { "R2C4", "AirbusFBW/MCDU{side}Prog" },
                    { "R3C4", "AirbusFBW/MCDU{side}Perf" },
                    { "R4C4", "AirbusFBW/MCDU{side}Init" },
                    { "R6C4", "AirbusFBW/MCDU{side}Data" },

                    { "R1C5", "AirbusFBW/MCDU{side}Fpln" },
                    { "R2C5", "AirbusFBW/MCDU{side}RadNav" },
                    { "R3C5", "AirbusFBW/MCDU{side}FuelPred" },
                    { "R4C5", "AirbusFBW/MCDU{side}SecFpln" },
                    { "R6C5", "AirbusFBW/MCDU{side}ATC" },
                    { "R7C5", "AirbusFBW/MCDU{side}Menu" },

                    { "R5C5", "AirbusFBW/MCDU{side}Airport" },

                    { "R1C6", "AirbusFBW/MCDU{side}SlewLeft" },
                    { "R2C6", "AirbusFBW/MCDU{side}SlewUp" },

                    { "R1C7", "AirbusFBW/MCDU{side}SlewRight" },
                    { "R2C7", "AirbusFBW/MCDU{side}SlewDown" },

                    // Numpad
                    { "R1C8", "AirbusFBW/MCDU{side}Key1" },
                    { "R2C8", "AirbusFBW/MCDU{side}Key2" },
                    { "R3C8", "AirbusFBW/MCDU{side}Key3" },

                    { "R1C9", "AirbusFBW/MCDU{side}Key4" },
                    { "R2C9", "AirbusFBW/MCDU{side}Key5" },
                    { "R3C9", "AirbusFBW/MCDU{side}Key6" },

                    { "R1C10", "AirbusFBW/MCDU{side}Key7" },
                    { "R2C10", "AirbusFBW/MCDU{side}Key8" },
                    { "R3C10", "AirbusFBW/MCDU{side}Key9" },

                    { "R1C11", "AirbusFBW/MCDU{side}KeyDecimal" },
                    { "R2C11", "AirbusFBW/MCDU{side}Key0" },
                    { "R3C11", "AirbusFBW/MCDU{side}KeyPM" },

                    // Letters
                    { "R4C6", "AirbusFBW/MCDU{side}KeyA" },
                    { "R5C6", "AirbusFBW/MCDU{side}KeyB" },
                    { "R6C6", "AirbusFBW/MCDU{side}KeyC" },
                    { "R7C6", "AirbusFBW/MCDU{side}KeyD" },
                    { "R8C6", "AirbusFBW/MCDU{side}KeyE" },

                    { "R4C7", "AirbusFBW/MCDU{side}KeyF" },
                    { "R5C7", "AirbusFBW/MCDU{side}KeyG" },
                    { "R6C7", "AirbusFBW/MCDU{side}KeyH" },
                    { "R7C7", "AirbusFBW/MCDU{side}KeyI" },
                    { "R8C7", "AirbusFBW/MCDU{side}KeyJ" },

                    { "R4C8", "AirbusFBW/MCDU{side}KeyK" },
                    { "R5C8", "AirbusFBW/MCDU{side}KeyL" },
                    { "R6C8", "AirbusFBW/MCDU{side}KeyM" },
                    { "R7C8", "AirbusFBW/MCDU{side}KeyN" },
                    { "R8C8", "AirbusFBW/MCDU{side}KeyO" },

                    { "R4C9", "AirbusFBW/MCDU{side}KeyP" },
                    { "R5C9", "AirbusFBW/MCDU{side}KeyQ" },
                    { "R6C9", "AirbusFBW/MCDU{side}KeyR" },
                    { "R7C9", "AirbusFBW/MCDU{side}KeyS" },
                    { "R8C9", "AirbusFBW/MCDU{side}KeyT" },

                    { "R4C10", "AirbusFBW/MCDU{side}KeyU" },
                    { "R5C10", "AirbusFBW/MCDU{side}KeyV" },
                    { "R6C10", "AirbusFBW/MCDU{side}KeyW" },
                    { "R7C10", "AirbusFBW/MCDU{side}KeyX" },
                    { "R8C10", "AirbusFBW/MCDU{side}KeyY" },

                    { "R4C11", "AirbusFBW/MCDU{side}KeyZ" },
                    { "R5C11", "AirbusFBW/MCDU{side}KeySlash" },
                    { "R6C11", "AirbusFBW/MCDU{side}KeySpace" },
                    { "R7C11", "AirbusFBW/MCDU{side}KeyOverfly" },
                    { "R8C11", "AirbusFBW/MCDU{side}KeyClear" },
                }
            }
        };

        // Per-aircraft LED bindings: dataref to watch, value that turns the LED on, and which Arduino pin to drive
        public static readonly Dictionary<string, List<LedBinding>> LedBindings = new Dictionary<string, List<LedBinding>>()
        {
            {
                "Toliss A320 family", new List<LedBinding>()
                {
                    new LedBinding { DataRef = "AirbusFBW/ADIRUOnBat", Condition = 1f, LedPin = 2 },
                    
                }
            }
        };
    }
}