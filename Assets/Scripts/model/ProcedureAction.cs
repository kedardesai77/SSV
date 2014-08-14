

public class ProcedureAction
{

    string guid;
    string insulation;
	string action;
	string description = "";
	
	//Getters
    public string       GetGUID()       { return guid;          }
    public string       GetInsulation() { return insulation;    }
	public string 		GetAction()		{ return action;		}
	public string 		GetDescription(){ return description;	}
	
	//Constructor
	public ProcedureAction( string proc_guid, string proc_action , string proc_description  )
	{
        guid = proc_guid;
		action = proc_action;
		description = proc_description ;
        insulation = proc_guid + "_I";
	}
	
	//Methods
	public bool Check( string proc_guid , string proc_action)
	{
		return guid.Equals(proc_guid) && action.Equals(proc_action);
	}
		
}