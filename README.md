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
![sampleImage](https://github.com/LightGive/UnityUtil/blob/image/ButtonToggleAttributeSample.gif?raw=true)


### EnumListLabel
`enum`の配列のラベルをEnumの名前に変更する

``` cs
public enum PiyoEnum
{
    Piyo,
    Hoge,
    Huga,
}

[SerializeField, EnumListLabel(typeof(PiyoEnum))] int[] _piyoArray;
```
![sampleImage](https://github.com/LightGive/UnityUtil/blob/image/EnumLabelListSample.png?raw=true)

### EnumFlags
`Flags`アトリビュートがついている`enum`をCameraのCullingMaskを選択する時のようにフラグで選択できるようにする

``` cs
[System.Flags]
public enum PiyoEnum
{
    Piyo,
    Hoge,
    Huga,
}

[SerializeField, EnumFlags] PiyoEnum _piyo;
```
![sampleImage](https://github.com/LightGive/UnityUtil/blob/image/EnumFlagsSample.png?raw=true)
