using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate_turbine : MonoBehaviour 
{
    public Animator anim;
    public bool check_disassembly;         // проверка на разборку
    
    public 	AudioClip star_sound;          // звук запуска установки
    public 	AudioClip work_sound;          // звук работы установки
    public 	AudioClip stop_sound;          // звук остановки установки
    
    public bool animation_play;            // запуск анимации  
    public CaseStepsTop CaseStepsTop; 
    bool work_clip;                        // запуск клипа работы гту
    bool stop_clip;		                     // запуск клипа остановки гту
    public AudioSource audio_work; 
    public AudioSource audio_source;       // контейнер для звукового клипа 
    string stop_hide;
    public Material case_top;              // обычный материал кожуха установки
    string unhide_case;

    // Вызывается только в первом кадре
    void Start()
    {
        anim = GetComponent<Animator>();   // инициализация аниматора
    }

    // Вызывается при каждом обновлении кадра сцены
    void Update()
    {
        if (animation_play && stop_hide != "stop_2")
            hideCaseTop();
        else if (!animation_play && stop_clip && unhide_case != "unhide_2") 
        {
            // Вернуть в сцену кожухи установки
            for (int i = 1; i <= 2; i++)
                unhide_case = CaseStepsTop.unhideCaseTop(CaseStepsTop.childe, i, case_top);
        }
        if (audio_source.time > 23 && !work_clip && !stop_clip) 
        {
            audio_work.Play();
            work_clip = true;
        }
        else if (stop_clip && audio_work.time > 10) 
        {
            audio_work.Stop();
            audio_source.Play();
        }

        if (work_clip && audio_work.time > 26.5)
            audio_work.Play();

        if (audio_source.time > 27 && stop_clip) 
        {
            animation_play = false;
            stop_hide = " ";
            audio_source.Stop();
        }
    }


    // метод вызывается при срабатывании кнопки запуска ГТУ
    public void Rotate_anim()
    {
        if (!check_disassembly) 
        {
            anim.Play("Start_turbine");
            audio_source.clip = star_sound;
            audio_source.Play();
            stop_clip = false;
            animation_play = true;
        }
    }

    // метод вызывается при срабатывании кнопки остановки ГТУ
    public void Rotate_stop()
    {
        if (!check_disassembly && work_clip) 
        {
            audio_source.clip = stop_sound;
            stop_clip = true;
            work_clip = false;
            
            anim.Play("Stop_turbine");
        }
    }

    // скрывает два кожуха в сцене
    void hideCaseTop()
    {
        for (int i = 1; i <= 2; i++)
            stop_hide = CaseStepsTop.TransparencyCase(CaseStepsTop.childe, i);
    }
}
