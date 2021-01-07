using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace BossHelper
{
    public class BossHelperSettings : ISettings
       {
        public BossHelperSettings()
        {
            Enable = new ToggleNode(false);
            FloatingIconZValue = new RangeNode<int>(-140, -300, 300);
            TestIconOnPlayer = new ToggleNode(false);
      
        }

        // Does not show on menu
        public ToggleNode Enable { get; set; }
        
        [Menu("Floating icon height", "Adjust Z height of floating icon.")]
        public RangeNode<int> FloatingIconZValue { get; set; }


        [Menu("Test icon", "Test floating icon on player.")]
        public ToggleNode TestIconOnPlayer { get; set; }

       }
    
    
    
}
