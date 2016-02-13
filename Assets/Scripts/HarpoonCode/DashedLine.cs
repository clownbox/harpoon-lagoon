using UnityEngine;
using System.Collections;

public class DashedLine : MonoBehaviour {
	public Material mat;
	public static Vector3 startVertex;
	public static Vector3 endVertex;
	bool drawingLine = false;
	public static bool enableHoldLine = false;

	/*void CreateLineMaterial() 
	{
		if( !mat ) {
			mat = new Material( "Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off Cull Off Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }" );
			mat.hideFlags = HideFlags.HideAndDontSave;
			mat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}*/

	void Start() {
		// CreateLineMaterial();
		startVertex = new Vector3(0, 0, 0);
	}

	void Update() {
		if(Input.GetMouseButtonDown(0)) {
			drawingLine = true;
		}
		if(Input.GetMouseButtonUp(0)) {
			drawingLine = false;
		}
	}
		
	void OnPostRender() {
		if(drawingLine == false || enableHoldLine==false) {
			return;
		}
		if (!mat) {
			Debug.LogError("material missing");
			return;
		}
		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.LINES);
		GL.Color(Color.red);

		if(ClickTouchReportToThrow.fixedLength == false) {
			GL.Vertex(startVertex);
			GL.Vertex(endVertex);
		} else {
			float dashes = 40.0f;
			float animSlide = Time.time - Mathf.FloorToInt(Time.time);
			Vector3 lineDelta = ((endVertex - startVertex).normalized * animSlide) / (dashes / 2.0f);
			for(int dash = 0; dash <= dashes; dash++) {
				GL.Vertex((startVertex * dash + endVertex * (dashes - dash)) / dashes + lineDelta);
			}
		}
		GL.End();
		GL.PopMatrix();
	}
}