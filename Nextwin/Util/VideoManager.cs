﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Nextwin.Util
{
    public class VideoManager : Singleton<VideoManager>
    {
        public delegate void Callback();

        [SerializeField, Header("Set your video resources path")]
        [Tooltip("If your videos are in Assets/Resources/Videos directory, set as \"Videos\"")]
        private string _videoResourcesPath;

        [SerializeField, Header("Key: Screen name / Value: Screen to play video (RawImage)")]
        private SerializableDictionary<string, RawImage> _screens;
        [SerializeField, Header("Key: VideoPlayer name / Value: VideoPlayer component")]
        private SerializableDictionary<string, VideoPlayer> _videoPlayers;
        private Dictionary<string, Texture> _textures;

        private void Start()
        {
            CheckComponentsAssigned();
        }

        /// <summary>
        /// 비디오를 재생, 비디오가 끝나면 특정 작업을 실행 시킬 수 있음
        /// </summary>
        /// <param name="videoNameWithDirectoryName">재생할 비디오의 이름(설정한 VideoResourcesPath의 하위 폴더 안에 파일이 있다면 SubDir/VideoName과 같이 명시)</param>
        /// <param name="videoPlayerName">비디오가 재생될 VideoPlayer 이름, VideoPlayer가 하나라면 생략 가능</param>
        /// <param name="callback">비디오가 끝난 후 실행할 작업</param>
        public void PlayVideo(string videoNameWithDirectoryName, string videoPlayerName = null, string screenName = null, Callback callback = null)
        {
            VideoPlayer player = GetVideoPlayer(videoPlayerName);
            if(player == null)
            {
                return;
            }

            VideoClip clip = Resources.Load<VideoClip>($"{_videoResourcesPath}/{videoNameWithDirectoryName}");
            if(clip == null)
            {
                Debug.LogError($"There is no video named {videoNameWithDirectoryName}");
                return;
            }

            RawImage screen = GetScreen(screenName);
            if(screen == null)
            {
                return;
            }

            ActionManager.Instance.ExecuteWithDelay(() =>
            {
                player.Play();
            }, () =>
            {
                StartCoroutine(FadeIn(screen));
            }, 0.5f);

            player.loopPointReached += (videoPlayer) =>
            {
                // 비디오가 끝난 후 waitAndCallback초 후에 callback 실행
                ActionManager.Instance.ExecuteWithDelay(() =>
                {
                    StartCoroutine(FadeOut(screen));
                }, () =>
                {
                    callback?.Invoke();
                }, 3f);
            };
        }

        /// <summary>
        /// 모든 VideoPlayer 일시정지
        /// </summary>
        public void PauseAll()
        {
            foreach(KeyValuePair<string, VideoPlayer> item in _videoPlayers)
            {
                item.Value.Pause();
            }
        }

        /// <summary>
        /// 특정 VideoPlayer 일시정지
        /// </summary>
        /// <param name="videoPlayerName">일시정지 시키려는 VideoPlayer 이름</param>
        public void Pause(string videoPlayerName)
        {
            _videoPlayers[videoPlayerName].Pause();
        }

        /// <summary>
        /// 모든 VideoPlayer 재생
        /// </summary>
        public void ResumeAll()
        {
            foreach(KeyValuePair<string, VideoPlayer> item in _videoPlayers)
            {
                item.Value.Play();
            }
        }

        /// <summary>
        /// 특정 VideoPlayer 재생
        /// </summary>
        /// <param name="videoPlayerName">일시정지 시키려는 VideoPlayer 이름</param>
        public void Resume(string videoPlayerName)
        {
            _videoPlayers[videoPlayerName].Play();
        }

        private void CheckComponentsAssigned()
        {
            if(_screens.Count == 0)
            {
                Debug.LogError("There is no screen assgined for VideoManager.");
            }
            if(_videoPlayers.Count == 0)
            {
                Debug.LogError("There is no VideoPlayer assgined for VideoManager.");
            }

            CheckTargetTextureAssigned();
        }

        private void CheckTargetTextureAssigned()
        {
            foreach(KeyValuePair<string, VideoPlayer> item in _videoPlayers)
            {
                Texture texture = item.Value.targetTexture;
                if(texture == null)
                {
                    Debug.LogError($"Assign target texture to {item.Key} VideoPlayer.");
                }
                _textures[item.Key] = texture;
            }
        }

        private VideoPlayer GetVideoPlayer(string videoPlayerName)
        {
            if(videoPlayerName == null)
            {
                foreach(KeyValuePair<string, VideoPlayer> item in _videoPlayers)
                {
                    return item.Value;
                }
            }

            if(!_videoPlayers.ContainsKey(videoPlayerName))
            {
                Debug.LogError($"There is no VideoPlayer which key is {videoPlayerName}");
                return null;
            }

            return _videoPlayers[videoPlayerName];
        }

        private RawImage GetScreen(string screenName)
        {
            if(screenName == null)
            {
                foreach(KeyValuePair<string, RawImage> item in _screens)
                {
                    return item.Value;
                }
            }

            if(!_screens.ContainsKey(screenName))
            {
                Debug.LogError($"There is no screen which key is {screenName}");
                return null;
            }

            return _screens[screenName];
        }

        private IEnumerator FadeIn(RawImage screen)
        {
            screen.gameObject.SetActive(true);
            Color32 color = screen.color;

            WaitForSeconds waitForSeconds = new WaitForSeconds(0.0001f);
            while(color.a < 255)
            {
                color.a += 3;
                screen.color = color;
                yield return waitForSeconds;
            }
        }

        private IEnumerator FadeOut(RawImage screen)
        {
            Color32 color = screen.color;

            WaitForSeconds waitForSeconds = new WaitForSeconds(0.0001f);
            while(color.a > 0)
            {
                color.a -= 3;
                screen.color = color;
                yield return waitForSeconds;
            }

            screen.gameObject.SetActive(false);
        }
    }
}
