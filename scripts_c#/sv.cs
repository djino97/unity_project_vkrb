using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CameraRotateAround : MonoBehaviour {
 
  public Interface Interface;
	public Transform target;
	public Vector3 offset;
	public float sensitivity = 3; // чувствительность мышки
	public float limit = 80; // ограничение вращения по Y
	public float zoom = 0.25f; // чувствительность при увеличении,    колесиком мышки
	public float zoomMax = 10; // макс. увеличение
	public float zoomMin = 2; // мин. увеличение
	private float X, Y;
	public bool translation_camera; // перемещение камеры
	private bool start_values;
    
	void Update ()
	{
	   if(Interface.hide_canvas && !start_values)
	   {
	      limit = Mathf.Abs(limit); // абсолютное значение угла
	      if(limit > 90) limit = 90;
	           offset = new Vector3(offset.x, offset.y, - Mathf.Abs(zoomMax) / 2); // начальная позиция камеры
	      start_values = true;
	   }
		
          // при зажатии кнопки мыши происходит смена позиции камеры
	  if(Input.GetMouseButtonDown(0))
	      translation_camera = true;
	  else if(Input.GetMouseButtonUp(0))
	      translation_camera = false;
			
	  if(Interface.hide_canvas && translation_camera)
	  {
              // Масштабирование изображения колесиком мыши
	      if(Input.GetAxis("Mouse ScrollWheel") > 0) 
       	      	   offset.z += zoom;
	      else if(Input.GetAxis("Mouse ScrollWheel") < 0) 
      		   offset.z -= zoom;
		  
     	      offset.z = Mathf.Clamp(offset.z, - Mathf.Abs(zoomMax), -  Mathf.Abs(zoomMin));
	      X = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity; // смена позиции по координате X
 
    	      // смена позиции по координате Y
   	      Y += Input.GetAxis("Mouse Y") * sensitivity; 
	      Y = Mathf.Clamp (Y, -limit, 0);
      	      transform.localEulerAngles = new Vector3(-Y, X, 0);
              transform.position = transform.localRotation * offset   +     target.position;
	  }
	}
}
