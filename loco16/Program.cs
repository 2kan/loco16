using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loco16
{
	class Program
	{
		static void Main ( string[] args )
		{
			var loco = new Loco();
			loco.ReadFile( "E:\\Games\\Steam\\steamapps\\common\\Locomotion\\Single Player Saved Games\\North America (East) 19502.SV5" );
			//loco.ReadFile( "E:\\Games\\Steam\\steamapps\\common\\Locomotion\\ObjData\\2EPB.DAT" );
			//loco.ReadFile( "C:\\Users\\Tom\\Documents\\git\\loco16\\loco16\\bin\\Debug\\North America (East) 19502.SV5" );
		}
	}
}
