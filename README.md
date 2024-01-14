# UnityUtil
For myself

## Attribute
### ButtonToggle
`bool`のフィールドをボタンの見た目のトグルに変更する 
``` cs
[SerializeField, ButtonToggle("Label", "ON", "OFF")] bool _piyo1;
[SerializeField, ButtonToggle("Label")] bool _piyo2;
[SerializeField, ButtonToggle("ON", "OFF")] bool _piyo3;
```
SampleImage<br>
![sampleImage](https://github.com/LightGive/UnityUtil/blob/image/ButtonToggleAttributeSample.gif?raw=true)