using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode] 

public class Interface : MonoBehaviour 
{
    // наследование других скриптов 
 
    public rotate_turbine RotateTurbin;
    public FlamePipes flame_pipes_t;
    public BoltsFlameTube1 BoltsFlameTube1;
    public FlameTube FlameTube;
    public CaseStepsTop CaseStepsTop;
    public Compressor Compressor;
    public DropdownSel DropdownSel;
    public ToggleDescription ToggleDescription;
    public BeginDisassembly BeginDisassembly;
    public CameraRotateAround CameraRotateAround;
	
    public Material select_click;                  // материал для подсветки детали
    public Material disk_material;                 // обычный материал для дисков компрессора 
    public Material combustion_material;           // материал для камеры сгорания
	  
    public GameObject HelpCompressor;              // подсказка для разборки компрессора 
    public GameObject HelpChamber;                 // подсказка для разборки компрессора
    private GameObject[] obj = new GameObject[14]; // массив для 14 камер сгорания
    public GameObject image_description;           // изображение детали
	
    RaycastHit hit;  
    Ray MyRay;                                     // объявления направленного луча при разборке установки
    Ray DescRay;                                   // объявления направленного луча при выводе описания детали установки
	  
    int number_chamber;
    MeshFilter filter_select = null;
    MeshFilter filter;
    public bool disk_compressor = false;
    bool select_chamber;
    bool check_material;
    GameObject[] obj_comdustion_chamber = new GameObject[14];
    Sprite sprite_load;
    MeshFilter target_detail;
    TextAsset text_load;                           // текст, содержащий описание деталей
	  
    public GameObject description_panel;
    public GameObject text_description;
    bool select_target;
    public GameObject interface_canvas; 
    public bool hide_canvas;                       // флаг на скрытие canvas
  
  
    // вписать в файл специальный булевский флаг
    
    public void WriteFile(string flag)
    {
        string DataPath = Application.dataPath + "/Resources/Text/Check";     // путь до файла

        //объявление нового экземпляра класса для записи в файл
        StreamWriter dataWriter = new StreamWriter(DataPath);
        dataWriter.WriteLine(flag); // вписать строку
        dataWriter.Flush(); // работа с буфером данных
        dataWriter.Close(); // закрывает записываемый файл
    }


    // считывает информацию из файла

    string ReadFile()
    {
        StreamReader dataRead = new StreamReader(Application.dataPath + "/Resources/Text/Check");
        string check_reset = dataRead.ReadLine();
        dataRead.Close();
        return check_reset;
    }


    void Start()
    {
        /*
          выполняется, когда в файле записан false и приложение запущено 
        */
        
        if (Application.isPlaying && ReadFile() == "False") 
        {
            for (int i = 0; i <= 13; i++) 
                obj_comdustion_chamber[i] = GameObject.Find("flame_tube_" + (i + 1).ToString());
            description_panel.SetActive(false);
            interface_canvas = GameObject.Find("Canvas");
            interface_canvas.SetActive(false);
            HelpCompressor.SetActive(false);
            HelpChamber.SetActive(false);
        }
        else if (Application.isPlaying) 
        {
            hide_canvas = true;
            transform.localPosition = new Vector3(-5.2f, 1.4f, 12.6f);
        }
    }


    void Update()
    {
        /* 
           Если приложение запущено, выполняется процесс смены позиции камеры 
           с скрытым интерфейсом, пока не достигнет требуемой позиции; если приложение не запущено, в файл пишется false 
        */

        if (!Application.isPlaying)
            WriteFile("False");
        else 
        {
            if (!hide_canvas) 
            {
                if (ReadFile() == "False") 
                {
                    float step = 1.2f * Time.deltaTime;
                    
                    transform.localPosition = 
                        Vector3.MoveTowards(transform.localPosition, new Vector3(-5.7f, 1.4f, 12.3f), step);
                    if (transform.localPosition == new Vector3(-5.7f, 1.4f, 12.3f)) 
                    {
                        hide_canvas = true;
                        interface_canvas.SetActive(true);
                    }
                }
            }
            else 
            {
                /* выбор нужного алгоритма  для выбранной пользователем детали */
                if (BeginDisassembly.disassembly && !RotateTurbin.animation_play) 
                {
                    RotateTurbin.check_disassembly = true;
                    switch (DropdownSel.select_component) 
                    {
                        case "Диск компрессора":
                            disk_compressor = true;
                            lame_pipes_t.start_translation = true;
                            DropdownSel.select_component = "";
                            break;
			    
                        case "Камера сгорания":
                            select_chamber = true;
                            BeginDisassembly.disassembly = false;
                            break;
                    }
                }

                TranslationCase();
                TranslationCompressor();
                CombustionChamber();


                // управление видимостью контекстных подсказок
                
                if (Compressor.select_disk && !Compressor.math_disk)
                    HelpCompressor.SetActive(true);
                else
                    HelpCompressor.SetActive(false);

                if (select_chamber)
                    HelpChamber.SetActive(true);
                else
                    HelpChamber.SetActive(false);


                /* если наступил момент выбора нужной детали с помощью мыши, вызывается метод с инициализацией направленного луча */

                if ((Compressor.select_disk || select_chamber) && 
                !Compressor.math_disk && !CameraRotateAround.translation_camera) 
                {
                    RaycastSelected(false);

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                        RaycastSelected(true);
                }
            }
        }

        /* если требуется вывести описание детали с помощью мыши, вызывается метод с инициализацией направленного луча */

        if (ToggleDescription.enable_description && !CameraRotateAround.translation_camera)
            RaycastDescription();
        else
            description_panel.SetActive(false);
    }

    //алгоритм перемещения кожуха установки

    void TranslationCase()
    {
        if (flame_pipes_t.check_translation() && disk_compressor) 
        {
            flame_pipes_t.start_translation = false;
            BoltsFlameTube1.translation_bolts_t1 = true;
            FlameTube.translation_flame_tube = true;
            
            FlameTube.option_disassembly = "flame_tube_top";
            BoltsFlameTube1.option_disassembly = "flame_tube_top";
            
            if (BoltsFlameTube1.stop_translation_case == true) 
            {
                CaseStepsTop.translation_input_turbine = true;
                FlameTube.translation_flame_tube = false;
            }
        }
    }


    // метод для перемещения компрессора установки

    void TranslationCompressor()
    {
        if (CaseStepsTop.stop_translation)
            Compressor.translation_start = true;
    }

    // метод для перемещения камер сгорания

    void CombustionChamber()
    {
        if (select_target) 
	{
            for (int i = 0; i <= 13; i++) 
	    {
                if ((13 <= i || i <= 3) && number_chamber == i) 
                {
                    if (flame_pipes_t.check_translation()) 
                    {
                        FlameTube.select_chamber = obj_comdustion_chamber[i];
                        BoltsFlameTube1.select_bolts = i;
                        
                        FlameTube.option_disassembly = "flame_tube_one";
                        BoltsFlameTube1.option_disassembly = "flame_tube_one";
                        
                        BoltsFlameTube1.translation_bolts_t1 = true;
                        FlameTube.translation_flame_tube = true;
                        select_target = false;
                    }
                    else
                        flame_pipes_t.start_translation = true;
                }
            }
        }
        else if (!disk_compressor)
            flame_pipes_t.start_translation = false;
    }

    void RaycastSelected(bool clicked)
    {
        //считывается позиция мышки это будет начальная точка луча
        MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(MyRay.origin, MyRay.direction * 10, Color.yellow);
        
        if (Physics.Raycast(MyRay, out hit, 100)) 
        {
            filter = hit.collider.GetComponent(typeof(MeshFilter)) as MeshFilter;

            if (filter) 
            {
                if ((select_chamber && filter.gameObject.tag == "combustion_chamber") || 
                (Compressor.select_disk && filter.gameObject.tag == "stage_compressor"))
                    filter.gameObject.GetComponent<MeshRenderer>().material = select_click;

                if (clicked) 
                {

                    if (Compressor.select_disk) 
                    {
                        //имя обьекта по которому щелкнули мышью
                        Compressor.selected_disk = filter.gameObject;
                        Compressor.math_disk = true;
                        Compressor.select_disk = false;
                        
                        filter_select.gameObject.GetComponent<MeshRenderer>().material = disk_material;
                    }
		    
                    if (select_chamber)
                        for (int i = 0; i <= 13; i++) 
                        {
                            if (obj_comdustion_chamber[i] == filter.gameObject) 
                            {
                                select_chamber = false;
                                number_chamber = i;
                                select_target = true;
                                
                                filter_select.gameObject.GetComponent<MeshRenderer>().material = combustion_material;
                            }
                        }
                }
            }
	    
            if (filter_select == null)
                filter_select = filter;

            if (filter != filter_select) 
            {
                if (Compressor.select_disk && filter.gameObject.tag == "stage_compressor") 
                {
                    filter_select.gameObject.GetComponent<MeshRenderer>().material = disk_material;
                    filter_select = filter;
                }
                else if (select_chamber && filter.gameObject.tag == "combustion_chamber") 
                {
                    filter_select.gameObject.GetComponent<MeshRenderer>().material = combustion_material;
                    filter_select = filter;
                }
            }
        }
    }

    void RaycastDescription()
    {
        // начальная точка луча и его направление
        DescRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(DescRay, out hit, 100))                                   // условие выполняется при пересечении луча с коллайдером
        {
            filter = hit.collider.GetComponent(typeof(MeshFilter)) as MeshFilter;     // передает данные об объекте, пересеченным лучом
            description_panel.SetActive(true);

            // выбор описания для детали
            switch (filter.gameObject.tag) 
            {
                case "combustion_chamber":
                    sprite_load = Resources.Load<Sprite>("Image/" + (filter.gameObject.tag).ToString());
                    text_load = Resources.Load<TextAsset>("Text/" + (filter.gameObject.tag).ToString());
                    break;
                    
                case "stage_compressor":
                    sprite_load = Resources.Load<Sprite>("Image/" + (filter.gameObject.tag).ToString());
                    text_load = Resources.Load<TextAsset>("Text/" + (filter.gameObject.tag).ToString());
                    break;
                    
               default:
                    sprite_load = null;
                    text_load = null;
                    break;
            }
            if (text_load != null)
                text_description.GetComponent<Text>().text = text_load.text;
            else
                text_description.GetComponent<Text>().text = null;
          	image_description.GetComponent<UnityEngine.UI.Image>().sprite = sprite_load;
        }
        else
            description_panel.SetActive(false);
    }
}
