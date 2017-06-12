using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourcesManager// : MonoBehaviour 
{

    static int id = 0;
    static AudioSourcesManager instance;

    public static AudioSourcesManager GetInstance() {
        if (instance == null) {
            instance = new AudioSourcesManager();
        }

        return instance;
    }


    public Dictionary<int, AudioSource> listener_list = new Dictionary<int, AudioSource>();

    public int AddAudioSource(AudioSource a) {
        id++;

        listener_list.Add(id, a);

        return id;
    }

    public void RemoveAudioSource(int id)
    {
        listener_list.Remove(id);
    }

    public void ChangeStatus(bool b) {
        foreach (KeyValuePair<int, AudioSource> kv in listener_list)
        {
            if (kv.Value != null) {
                //kv.Value.enabled = b;

                kv.Value.mute = !b;
            }
        }
    }

    //Dictionary<string, AudioClip> clip_list = new Dictionary<string, AudioClip>();

    public void Play(AudioSource audio , AudioClip clip , bool is_loop = false) {
        if (audio != null && clip != null) {
            if (audio.isPlaying) {
                audio.Stop();
                //audio.clip
            }

            //AudioClip clip;

            /*
            if (clip_list.ContainsKey(name))
            {
                clip = clip_list[name];
            }
            else {
                clip = (AudioClip)Resources.Load("Musics/" + name, typeof(AudioClip));

                clip_list.Add(name,clip);
            }
            */

            audio.clip = clip;
            audio.loop = is_loop;
            audio.Play();
        }
    }

    public void Stop(AudioSource audio)
    {
        if (audio != null)
        {
            if (audio.isPlaying)
            {
                audio.Stop();
                //audio.clip
            }
        }
    }

	// Use this for initialization
	//void Start () {
		
	//}
	
	// Update is called once per frame
	//void Update () {
		
	//}
}
