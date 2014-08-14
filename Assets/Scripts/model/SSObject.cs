using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class SSObject : MonoBehaviour
{
	
	public List<Material[]> original_materials;
	public List<Material[]> saved_materials;
	
	public bool isFaded = false;
	public bool isHidden = false;
	
	public bool isInsulation;
	

	public static Shader transparent_Diffuse_Shader = Shader.Find("Transparent/Diffuse");
	public static Shader diffuse_Shader =Shader.Find("Diffuse");	
	public static Shader outlined_sillhouette_shader = Shader.Find("Outlined/Silhouetted Diffuse");
	
	
	Material[] new_mats;
	
	void Start ()
	{
		original_materials = new List<Material[]>();
		saved_materials = new List<Material[]>();
		
		MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
		for(int i = 0 ; i < children.Length ; i++)
		{
			GameObject go = children[i].gameObject;
			Material[] mats = go.renderer.materials;
			Material[] temp_original_materials = new Material[mats.Length];
			Material[] temp_saved_materials = new Material[mats.Length];
			
			for(int j = 0; j < mats.Length ; j++)
			{
				temp_original_materials[j] = new Material(go.renderer.materials[j]);
				temp_saved_materials[j] = new Material(go.renderer.materials[j]);
			}	
			original_materials.Add(temp_original_materials);
			saved_materials.Add(temp_saved_materials);
		}
		if(gameObject.name.EndsWith("_I"))
		{
			isInsulation = true;
            gameObject.layer = (int)SafeSim.Layers.model;
			gameObject.tag = "Insulation";
		}
		else
		{
			isInsulation = false;
			gameObject.layer = (int)SafeSim.Layers.model;
			gameObject.tag = "Model";
		}
		
	}
	
	public Material[] Get_Mat_Colors()
	{
		Material[] mats = new Material[renderer.materials.Length];
		for(int i=0; i< mats.Length;i++)
		{
			mats[i] = renderer.materials[i];
			
		}
		return mats;
	}

    public Color Get_Origianl_Color()
    {
       return original_materials[0][0].color;
    }
	
	public void Set_Mat_Color(Color color , bool commit = false)
	{	
		
		if(GetComponent<MeshRenderer>() == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				//Change to selection color, maintaining alpha
				Color c = new Color(color.r, color.g, color.b, go.renderer.material.color.a);	
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					go.renderer.materials[j].color = c;
					if(commit)
					{
						saved_materials[i][j].color = c;	
					}
					//material.shader = toon;
				}					
			}
		}
		else
		{
			
			//Change to selection color, maintaining alpha
			Color c = new Color(color.r, color.g, color.b, renderer.material.color.a);				
			
			//Apply color
			for(int i = 0; i < renderer.materials.Length ; i++)
			{
				renderer.materials[i].color = c;
				if(commit)
				{
					saved_materials[0][i].color = c;	
				}
				//material.shader = toon;
			}	
		}
	}
	
	public static void Set_Mat_Color(Color color , GameObject guid)
	{	
		
		if(guid.GetComponent<MeshRenderer>() == null)
		{
			MeshRenderer[] children = guid.GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				Color c = new Color(color.r, color.g, color.b, go.renderer.material.color.a);	
				for(int j = 0; j < go.renderer.materials.Length ; j++)
					go.renderer.materials[j].color = c;				
			}
		}
		else
		{
			Color c = new Color(color.r, color.g, color.b, guid.renderer.material.color.a);				
			for(int i = 0; i < guid.renderer.materials.Length ; i++)
				guid.renderer.materials[i].color = c;
			
		}
	}	

	public void Set_Current_Mats(Color c , bool excludeAlpha = false)
	{
		if(GetComponent<MeshRenderer>() == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					if(excludeAlpha)
						saved_materials[i][j].color = new Color(c.r , c.g , c.b , go.renderer.materials[j].color.a);
					else
						saved_materials[i][j].color = c;
				}					
				
			}
		}
		else
		{
			foreach(Material mat in saved_materials[0])
				
				if(excludeAlpha)
					mat.color = new Color(c.r , c.g , c.b , mat.color.a);
			else
				mat.color = c;
		}
	}
	
	public void Reset_Colors()
	{
		if(GetComponent<MeshRenderer>() == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].transform.gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					Color c = saved_materials[i][j].color;
					go.renderer.materials[j].color = new Color(c.r,c.g,c.b,go.renderer.materials[j].color.a);
				}				
				
				
				
			}
		}
		else
		{
			
			for(int i = 0; i < saved_materials[0].Length ; i++)
			{
				Color c = saved_materials[0][i].color;
				renderer.materials[i].color = new Color(c.r,c.g,c.b,renderer.materials[i].color.a);
			}			
			
		}
		
		
	}
	public void Reset_Original_Colors()
	{
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;


				for(int j = 0; j < original_materials[i].Length ; j++)
				{
					go.renderer.materials[j].color = original_materials[i][j].color;
					go.renderer.materials[j].shader  = original_materials[i][j].shader;
					saved_materials[i][j].color = original_materials[i][j].color;
					saved_materials[i][j].shader = original_materials[i][j].shader;
				}
				
			}
		}
		else
		{		
			for(int i = 0; i < original_materials[0].Length ; i++)
			{
				renderer.materials[i].color = original_materials[0][i].color;
				renderer.materials[i].shader = original_materials[0][i].shader;
				saved_materials[0][i].color = original_materials[0][i].color;
				saved_materials[0][i].shader = original_materials[0][i].shader;
			}

		}
	}
	
	public void NetHighlight()
	{
		if(isHidden)return;
		
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			
			foreach(MeshRenderer child in children)
				foreach(Material m in child.renderer.materials)
					m.shader = outlined_sillhouette_shader;
		}
		else
		{
			foreach(Material material in renderer.materials)
				material.shader = outlined_sillhouette_shader;	
			
		}
		
	}
	public void NetUnhighlight()
	{
		if(isHidden)return;
		
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer child in children)
				foreach(Material m in child.renderer.materials)
					m.shader = diffuse_Shader;
		}
		else
		{
			foreach(Material material in renderer.materials) 
				material.shader = diffuse_Shader;	
		}		
		
		
	}
	
	public void Fade()
	{
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					go.renderer.materials[j].shader = transparent_Diffuse_Shader;
					Color tmp = go.renderer.materials[j].color;
					go.renderer.materials[j].color = new Color(tmp.r , tmp.g , tmp.b , 0.40f);
				}
				
			}
			
		}
		else
		{
			foreach(Material material in renderer.materials)
			{
				material.shader = transparent_Diffuse_Shader;	
				Color tmp = material.color;
				material.color = new Color(tmp.r , tmp.g , tmp.b , 0.40f);
			}
			
		}
		gameObject.layer = (int) SafeSim.Layers.Faded;

		isFaded = true;		
		
	}
	public void UnFade()
	{
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					go.renderer.materials[j].shader = diffuse_Shader;
					Color tmp = go.renderer.materials[j].color;
					go.renderer.materials[j].color = new Color(tmp.r , tmp.g , tmp.b , 1.0f);
				}
				
			}
			
		}
		else
		{		
			foreach(Material material in renderer.materials)
			{
				material.shader = diffuse_Shader;	
				Color tmp = material.color;
				material.color = new Color(tmp.r , tmp.g , tmp.b , 1.0f);
			}
		}
		gameObject.layer = (int) SafeSim.Layers.model;

		
		
		isFaded = false;
	}
	
	public void Hide()
	{
		
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					go.renderer.materials[j].shader = transparent_Diffuse_Shader;
					Color tmp = go.renderer.materials[j].color;
					go.renderer.materials[j].color = new Color(tmp.r , tmp.g , tmp.b , 0.0f);
				}
				
			}
			
		}
		else
		{			
			
			foreach(Material material in renderer.materials)
			{
				material.shader = transparent_Diffuse_Shader;	
				Color tmp = material.color;
				material.color = new Color(tmp.r , tmp.g , tmp.b , 0.0f);
			}
		}
		gameObject.layer = (int) SafeSim.Layers.Hidden;
		//SafeSim.fadedObjects.Add(name);
		
		isHidden = true;
	}
	public void UnHIde()
	{
		
		if(renderer == null)
		{
			MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
			for(int i = 0 ; i < children.Length ; i++)
			{
				GameObject go = children[i].gameObject;
				for(int j = 0; j < go.renderer.materials.Length ; j++)
				{
					go.renderer.materials[j].shader = transparent_Diffuse_Shader;
					Color tmp = go.renderer.materials[j].color;
					go.renderer.materials[j].color = new Color(tmp.r , tmp.g , tmp.b , 1.0f);
				}
				
			}
			
		}
		else
		{			
			
			foreach(Material material in renderer.materials)
			{
				material.shader = diffuse_Shader;	
				material.color = new Color(material.color.r , material.color.g , material.color.b , 1.0f);
			}
		}
		gameObject.layer = (int) SafeSim.Layers.model;
		
		//	SafeSim.fadedObjects.Remove(go.name);		
		
		isHidden = false;
		
		if(isFaded)	Fade();
	}
	
	public void Highlight()
	{
        //GetComponent<HighlightableObject>().On(Color.blue);
        
		if(renderer == null)
		{
			foreach(MeshRenderer child in GetComponentsInChildren<MeshRenderer>())
			{
				GameObject go = child.gameObject;
				foreach(Material mat in go.renderer.materials)
				{
					mat.color = new Color(mat.color.r + 0.15f , mat.color.g + 0.15f , mat.color.b + 0.15f , mat.color.a);
				}
			}
		}
		else
		{
			foreach(Material mat in renderer.materials)
			{
				mat.color = new Color(mat.color.r + 0.15f , mat.color.g + 0.15f , mat.color.b + 0.15f , mat.color.a);
			}
			
		}
         
	}
	public void UnHighlight()
	{

        //GetComponent<HighlightableObject>().Off();
        
		if(renderer == null)
		{
			foreach(MeshRenderer child in GetComponentsInChildren<MeshRenderer>())
			{
				GameObject go = child.gameObject;
				foreach(Material mat in go.renderer.materials)
				{
					mat.color = new Color(mat.color.r - 0.15f , mat.color.g - 0.15f , mat.color.b - 0.15f , mat.color.a);
				}
			}
		}
		else
		{		
			foreach(Material mat in renderer.materials)
			{
				mat.color = new Color(mat.color.r - 0.15f , mat.color.g - 0.15f , mat.color.b - 0.15f , mat.color.a);
			}
		}
         
	}



    internal void Tween_Mat_Color(Color c , bool p )
    {
        StartCoroutine(Tween_and_Set(c,p ));
    }

    private IEnumerator Tween_and_Set(Color c , bool p )
    {
        iTween.ColorTo(gameObject, c, 0.25f);
        yield return new WaitForSeconds(0.25f);
        Set_Mat_Color(c, p);

    }
}

