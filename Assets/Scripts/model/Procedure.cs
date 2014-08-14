using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Procedure
{
	
	int proj_id , id;
	public string name = "";
	public string description = "";


    public Dictionary<int, ProcedureAction> steps = new Dictionary<int, ProcedureAction>();
    private List<string> interactiveGUIDs = new List<string>();


	//Getters
	public string GetName()
	{
		return name;
	}
    public int GetId()
    {
        return id;
    }
	
	public List<string> GetInteractiveGUIDs()
	{
		return interactiveGUIDs; 
	}
	public int GetStepCount(){
		return steps.Count ;
	}
	public Dictionary<int , ProcedureAction> GetSteps()
	{
		return steps;	
	}
	public void SetDescription(string str)
	{
		description = str;	
	}
	public string GetDescription()
	{
		return description;	
	}
	
	//Setters
	public void SetName(string name)
	{
		this.name = name;	
	}

	//Constructors
	public Procedure(int id , string name , string description)
	{
		this.id = id;
		this.name = name;
		this.description = description;
	}	
	public Procedure(int proj_id)
	{
		this.proj_id = proj_id ;
	}
	public Procedure(int proj_id , int proc_num)
	{
		this.proj_id = proj_id ;
		this.id = proc_num ;
		//RetrieveProcedure(proc_num);
		//stepCount = steps.Count;
	}
	
	//Methods
	public bool Contains(string guid)
	{
		bool result = false;
		
		foreach(ProcedureAction pa in steps.Values)
			if(pa.GetGUID().Equals(guid))
				result = true;
		
		return result || interactiveGUIDs.Contains(guid);
	}
	public string GetAction(int step)
	{
		return steps[step].GetAction();	
	}
	public bool CheckAction( string guid , string action , int step)
	{
		return steps[step].Check( guid , action);
	}

    public IEnumerator LoadProcedure()
    {
        //Procedure procedure = new Procedure(procedureIndex, procedureList[procedureIndex].name, "");
        WWWForm form = new WWWForm();
        form.AddField("sa", "get_procedure");
        form.AddField("id", id.ToString());
        WWW www = new WWW(SafeSim.URL + "get_handler.ashx", form);
        yield return www;

        if (www.error == null)
        {
            string[] rows = www.text.Split('\n');
            foreach (string row in rows)
            {
                if (row.Length > 0)
                {
                    string[] cols = row.Split('|');
                    int order = int.Parse(cols[0]);
                    string guid = cols[1];
                    string state = cols[2];
                    string comment = cols[3];

                    ProcedureAction pa = new ProcedureAction(guid, state, comment);

                    steps.Add(order, pa);
                }
            }
        }

    }



    public IEnumerator Delete()
    {
        WWWForm form = new WWWForm();
        form.AddField("sa", "delete_procedure");
        form.AddField("id", id.ToString());
        WWW www = new WWW(SafeSim.URL + "save_handler.ashx", form);
        yield return www;
    }
}


