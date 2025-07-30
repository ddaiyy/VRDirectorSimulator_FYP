// 文件路径建议：Assets/Scripts/Selection/ICustomSelectable.cs

namespace MyGame.Selection
{
    public interface ICustomSelectable
    {
        void OnSelect();
        void OnDeselect();
    }
}
