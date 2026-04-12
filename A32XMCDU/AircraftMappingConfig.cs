using System.Collections.Generic;

namespace A32XMCDU
{
    public static class AircraftMappingConfig
    {
        public static readonly Dictionary<string, Dictionary<string, string>> Mappings = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "Toliss A321", new Dictionary<string, string>()
                {
                    { "R3C1", "toliss_airbus/mcdu/button/lsk1l" },
                    { "R3C2", "toliss_airbus/mcdu/button/lsk2l" },
                    { "R1C4", "toliss_airbus/mcdu/button/dir" }
                }
            },
            {
                "Toliss A320", new Dictionary<string, string>()
                {
                    { "R3C1", "toliss_airbus/mcdu/button/lsk1l" },
                    { "R3C2", "toliss_airbus/mcdu/button/lsk2l" },
                    { "R1C4", "toliss_airbus/mcdu/button/dir" }
                }
            }
        };
    }
}