using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARTrackedImageManager))]
public class ARDetectImage : MonoBehaviour
{
    [SerializeField] ModelInfo[] _modelsInfo;

    public UnityEvent<ModelInfo> OnDetect;


    private ARTrackedImageManager _imageManager;
    private Dictionary<string, ModelInfo> _imageDictionary;


    private void Awake()
    {
        _imageManager = GetComponent<ARTrackedImageManager>();

    }

    private void Start()
    {
     
        _imageDictionary = new Dictionary<string, ModelInfo>();
        for (int i = 0; i < _modelsInfo.Length; i++)
        {
            GameObject tempModel = Instantiate(_modelsInfo[i].model);
            tempModel.name = _modelsInfo[i].model.name;
            tempModel.SetActive(false);

            ModelInfo tempInfo = _modelsInfo[i];
            tempInfo.model = tempModel;
            tempInfo.isActivated = false;

            _imageDictionary.Add(tempModel.name, tempInfo);
        }
    }

    private void OnEnable()
    {
        _imageManager.trackedImagesChanged += ImageFound;
    }

    private void OnDisable()
    {
        _imageManager.trackedImagesChanged -= ImageFound;
    }

    private void ImageFound(ARTrackedImagesChangedEventArgs eventData)
    {
        foreach (var trackedImage in eventData.added)
        {
            ChangeModelState(trackedImage, true);
        }

        foreach (var trackedImage in eventData.updated)
        {
            switch(trackedImage.trackingState)
            {
                case TrackingState.Tracking:
                    ChangeModelState(trackedImage, true);
                    break;
                case TrackingState.Limited:
                    ChangeModelState(trackedImage, false);
                    break;
                case TrackingState.None:
                    break;
            }
        }

        foreach (var trackedImage in eventData.removed)
        {
            ChangeModelState(trackedImage, false);
        }
    }

    private void ChangeModelState(ARTrackedImage trackedImage, bool show)
    {
        GameObject currentModel = _imageDictionary[trackedImage.referenceImage.name].model;
        currentModel.transform.position = trackedImage.transform.position;


        if(show)
        {
            if(!IsModelActivated(trackedImage))
            {
                currentModel.SetActive(true);
                ModelInfo tempInfo = _imageDictionary[trackedImage.referenceImage.name];
                tempInfo.model = currentModel;
                tempInfo.isActivated = true;

                _imageDictionary[trackedImage.referenceImage.name] = tempInfo;

                OnDetect.Invoke(tempInfo);
            }
        }
        else
        {
            if (IsModelActivated(trackedImage))
            {
                currentModel.SetActive(false);
                ModelInfo tempInfo = _imageDictionary[trackedImage.referenceImage.name];
                tempInfo.model = currentModel;
                tempInfo.isActivated = false;

                _imageDictionary[trackedImage.referenceImage.name] = tempInfo;
            }
        }
    }

    private bool IsModelActivated(ARTrackedImage trackedImage)
    {
        return _imageDictionary[trackedImage.referenceImage.name].isActivated;
    }
}
