// Decompiled with JetBrains decompiler
// Type: Microsoft.Data.Visualization.Engine.ColorOperations
// Assembly: VisualizationEngine, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
// MVID: D1CA6C2A-5AF8-4816-98B2-7B03B8D226FF
// Assembly location: D:\Power Map Excel Add-in\Power Map Excel Add-in\x86\VISUALIZATIONENGINE.DLL

using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.Engine.VectorMath;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Visualization.Engine
{
  internal class ColorOperations : DisposableResource
  {
    private List<ColorOperation> operations = new List<ColorOperation>();
    private Matrix4x4F transform = Matrix4x4F.Identity;
    private bool updatedMatrix;
    private bool updatedColor;
    private bool updatedMap;
    private Texture rampTexture;
    private Tuple<Color4F, Color4F> colorReplace;

    public void AddOperation(ColorOperation operation)
    {
      this.operations.Add(operation);
      this.updatedMatrix = this.updatedColor = this.updatedMap = true;
    }

    public void Clear()
    {
      foreach (DisposableResource disposableResource in this.operations)
        disposableResource.Dispose();
      if (this.operations.Count > 0)
        this.updatedMatrix = this.updatedColor = this.updatedMap = true;
      this.operations.Clear();
    }

    public Matrix4x4F GetMatrix()
    {
      if (this.updatedMatrix)
      {
        this.updatedMatrix = false;
        Matrix4x4D matrix4x4D = Matrix4x4D.Identity;
        for (int index = 0; index < this.operations.Count; ++index)
        {
          ColorTransform colorTransform = this.operations[index] as ColorTransform;
          if (colorTransform != null)
            matrix4x4D *= colorTransform.GetMatrix();
        }
        this.transform = (Matrix4x4F) matrix4x4D;
      }
      return this.transform;
    }

    public Texture GetRampTexture(Renderer renderer)
    {
      if (this.updatedMap)
      {
        this.updatedMap = false;
        this.rampTexture = (Texture) null;
        for (int index = 0; index < this.operations.Count; ++index)
        {
          ColorMap colorMap = this.operations[index] as ColorMap;
          if (colorMap != null)
          {
            this.rampTexture = colorMap.GetTexture(renderer);
            break;
          }
        }
      }
      return this.rampTexture;
    }

    public Tuple<Color4F, Color4F> GetColorReplacement()
    {
      if (this.updatedColor)
      {
        this.updatedColor = false;
        this.colorReplace = (Tuple<Color4F, Color4F>) null;
        for (int index = 0; index < this.operations.Count; ++index)
        {
          ColorReplace colorReplace = this.operations[index] as ColorReplace;
          if (colorReplace != null)
            this.colorReplace = new Tuple<Color4F, Color4F>(colorReplace.Source, colorReplace.Destination);
        }
      }
      return this.colorReplace;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      foreach (DisposableResource disposableResource in this.operations)
        disposableResource.Dispose();
    }
  }
}
