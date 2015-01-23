using System;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  [Serializable]
  public class LabelDecoratorModel : DecoratorContentBase
  {
    private RichTextModel _Title = new RichTextModel()
    {
      FontSize = 26,
      Color = RichTextModel.DefaultTextColor.Key
    };
    private RichTextModel _Description = new RichTextModel()
    {
      FontSize = 12,
      Color = RichTextModel.DefaultTextColor.Key
    };

    public string PropertyTitle
    {
      get
      {
        return "Title";
      }
    }

    public RichTextModel Title
    {
      get
      {
        return this._Title;
      }
      set
      {
        base.SetProperty<RichTextModel>(this.PropertyTitle, ref this._Title, value);
      }
    }

    public string PropertyDescription
    {
      get
      {
        return "Description";
      }
    }

    public RichTextModel Description
    {
      get
      {
        return this._Description;
      }
      set
      {
        base.SetProperty<RichTextModel>(this.PropertyDescription, ref this._Description, value);
      }
    }

    public LabelDecoratorModel Clone()
    {
      return new LabelDecoratorModel()
      {
        Title = this.Title.Clone(),
        Description = this.Description.Clone()
      };
    }
  }
}
