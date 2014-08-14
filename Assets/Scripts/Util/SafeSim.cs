using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SafeSim 
{

	//EDGE Access
	public static string URL = "http://localhost:51606/UnitySafeSim/";
    //public static string URL = "http://localhost:51779/UnitySafeSim/";

    //public static string URL = "http://beta.image-edge360.com/UnitySafeSim/";
    public static string MODEL_URL = "http://cloud.image-edge360.com/UnityModels/";
	//blic static string url = "http://dev.image-edge360.com/SafeSim/";

    //public static string URL = "http://localhost:51777/UnitySafeSim";
    //public static string MODEL_URL = "http://beta.image-edge360.com/UnityModels/";


	public static string USERNAME = "";
	public static string PROJECT = "9007";
	public static string ASSET = "";
    public static Procedure PROCEDURE;

	public enum Modes{ viewer , test , demo , record , edit }
	public enum Layers{  model = 8 , Faded = 9 , Hidden = 10 , Controller = 11 , Insulation = 12};
	public static Modes CURRRENT_MODE = Modes.viewer;

	public static Regex guidRegex = new Regex("[0-9A-Z-]{36}");
	public static Regex dupRegex = new Regex("[0-9A-Z-]{36} \\d");

    public static bool prompt = false;






	


}
