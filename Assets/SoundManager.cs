using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance { get; private set; }

	[Header("Audio Sources")]
	[SerializeField]
	private AudioSource sfxSource;

	[SerializeField]
	private AudioSource musicSourceA;

	[SerializeField]
	private AudioSource musicSourceB;

	[SerializeField]
	private AudioSource countdownSource;

	[SerializeField]
	private AudioSource winSource;

	private AudioSource activeMusicSource;
	private float musicVolume = 1f;


	[Header("Arrow Hit Line Clips (RR01/RR02/RR03)")]
	[SerializeField]
	private List<AudioClip> arrowHitLineClips = new List<AudioClip>();

	[Header("Arrow Hit Target Clips (RR01/RR02/RR03)")]
	[SerializeField]
	private List<AudioClip> arrowHitTargetClips = new List<AudioClip>();

	[Header("Bow Pull Clips (RR01/RR02/RR03)")]
	[SerializeField]
	private List<AudioClip> bowPullClips = new List<AudioClip>();

	[Header("Bow Release Clips (RR01/RR02/RR03)")]
	[SerializeField]
	private List<AudioClip> bowReleaseClips = new List<AudioClip>();

	[Header("Background Music Clips")]
	[SerializeField]
	private AudioClip bgMusic1;

	[SerializeField]
	private AudioClip bgMusic2;

	[Header("Result Clips")]
	[SerializeField]
	private AudioClip winClip;

	[SerializeField]
	private AudioClip loseClip;

	[SerializeField]
	private AudioClip drawClip;



	[Header("Numeric Countdown Clips (3,2,1)")]
	[SerializeField]
	private AudioClip count1Clip; // named "1"

	[SerializeField]
	private AudioClip count2Clip; // named "2"

	[SerializeField]
	private AudioClip count3Clip; // named "3"

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			if (sfxSource == null)
			{
				sfxSource = GetComponent<AudioSource>();
				if (sfxSource == null)
				{
					sfxSource = gameObject.AddComponent<AudioSource>();
					sfxSource.playOnAwake = false;
					sfxSource.spatialBlend = 0f;
					sfxSource.dopplerLevel = 0f;
				}
			}

			// if (musicSourceA == null)
			// {
			// 	musicSourceA = gameObject.AddComponent<AudioSource>();
			// 	musicSourceA.playOnAwake = false;
			// 	musicSourceA.loop = true;
			// 	//musicSourceA.volume = musicVolume;
			// 	//musicSourceA.spatialBlend = 0f;
			// 	//musicSourceA.dopplerLevel = 0f;
			// }

			// if (musicSourceB == null)
			// {
			// 	musicSourceB = gameObject.AddComponent<AudioSource>();
			// 	musicSourceB.playOnAwake = false;
			// 	musicSourceB.loop = true;
			// 	//musicSourceB.volume = musicVolume;
			// 	//musicSourceB.spatialBlend = 0f;
			// 	//musicSourceB.dopplerLevel = 0f;
			// }

			if (countdownSource == null)
			{
				countdownSource = gameObject.AddComponent<AudioSource>();
				countdownSource.playOnAwake = false;
				countdownSource.loop = false;
				countdownSource.spatialBlend = 0f;
				countdownSource.dopplerLevel = 0f;
			}

			if (winSource == null)
			{
				winSource = gameObject.AddComponent<AudioSource>();
				winSource.playOnAwake = false;
				winSource.loop = false;
				winSource.spatialBlend = 0f;
				winSource.dopplerLevel = 0f;
			}

			activeMusicSource = musicSourceA;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void PlayMusic(AudioClip musicClip, bool loop = true)
	{
		if (musicClip == null)
		{
			return;
		}
		AudioSource target = GetInactiveMusicSource();
		target.loop = loop;
		if (target.clip == musicClip && target.isPlaying)
		{
			return;
		}
		target.clip = musicClip;
		//target.volume = musicVolume;
		target.Play();
		// Immediately switch active to target and mute the other
		AudioSource other = GetActiveMusicSource();
		///other.volume = 0f;
		activeMusicSource = target;
	}

	public void PlayWin()
	{
		if (winSource == null || winClip == null)
		{
			return;
		}
		winSource.PlayOneShot(winClip);
	}

	public void PlayLose()
	{
		if (winSource == null || loseClip == null)
		{
			return;
		}
		winSource.PlayOneShot(loseClip);
	}

	public void PlayDraw()
	{
		if (winSource == null || drawClip == null)
		{
			return;
		}
		winSource.PlayOneShot(drawClip);
	}

	public void PlayRandomArrowHitLine()
	{
		if (arrowHitLineClips == null || arrowHitLineClips.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, arrowHitLineClips.Count);
		AudioClip clip = arrowHitLineClips[index];
		if (clip == null)
		{
			return;
		}
		sfxSource.PlayOneShot(clip);
	}

	public void PlayRandomArrowHitTarget()
	{
		if (arrowHitTargetClips == null || arrowHitTargetClips.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, arrowHitTargetClips.Count);
		AudioClip clip = arrowHitTargetClips[index];
		if (clip == null)
		{
			return;
		}
		sfxSource.PlayOneShot(clip);
	}

	public void PlayRandomBowPull()
	{
		if (bowPullClips == null || bowPullClips.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, bowPullClips.Count);
		AudioClip clip = bowPullClips[index];
		if (clip == null)
		{
			return;
		}
		sfxSource.PlayOneShot(clip);
	}

	public void PlayRandomBowRelease()
	{
		if (bowReleaseClips == null || bowReleaseClips.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, bowReleaseClips.Count);
		AudioClip clip = bowReleaseClips[index];
		if (clip == null)
		{
			return;
		}
		sfxSource.PlayOneShot(clip);
	}

	

	private AudioSource GetActiveMusicSource()
	{
		return activeMusicSource == null ? musicSourceA : activeMusicSource;
	}

	private AudioSource GetInactiveMusicSource()
	{
		if (activeMusicSource == null)
		{
			return musicSourceB;
		}
		return activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
	}

	public void PlayBgMusic1()
	{
		if (bgMusic1 != null)
		{
			PlayMusic(bgMusic1, true);
		}
	}

	public void PlayBgMusic2()
	{
		if (bgMusic2 != null)
		{
			PlayMusic(bgMusic2, true);
		}
	}
	// Play individual countdown clips (3, 2, 1)
	public void PlayCountdown3()
	{
		if (countdownSource != null && count3Clip != null)
		{
			countdownSource.PlayOneShot(count3Clip);
		}
	}

	public void PlayCountdown2()
	{
		if (countdownSource != null && count2Clip != null)
		{
			countdownSource.PlayOneShot(count2Clip);
		}
	}

	public void PlayCountdown1()
	{
		if (countdownSource != null && count1Clip != null)
		{
			countdownSource.PlayOneShot(count1Clip);
		}
	}

}


