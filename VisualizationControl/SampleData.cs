using Microsoft.Data.Visualization.Engine;
using Microsoft.Data.Visualization.Engine.Graphics;
using Microsoft.Data.Visualization.VisualizationCommon;
using Microsoft.Data.Visualization.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Data.Visualization.VisualizationControls
{
  public static class SampleData
  {
    private static FieldListPickerViewModel _DD_FieldListPickerViewModel = new FieldListPickerViewModel((GeoVisualization) null, (IDialogServiceProvider) null)
    {
      InstructionText = "Choose the field(s) that make up the geography you would like to visualize.",
      State = FieldListPickerState.ChooseVisField
    };
    private static TableIslandViewModel _DD_TableIslandViewModel = new TableIslandViewModel();
    private static TableViewModel _DD_TableViewModel = new TableViewModel()
    {
      Name = "Table 1"
    };
    private static HostControlViewModel _DD_HostWindowViewModel;
    private static LayerViewModel _DD_LayerViewModel;
    private static LayerManagerViewModel _DD_LayerManagerViewModel;
    private static FieldWellGeographyViewModel _DD_FieldWellGeographyViewModel;
    private static FieldWellVisualizationViewModel _DD_FieldWellVisualizationViewModel;
    private static StatusBarViewModel _DD_StatusBarViewModel;
    private static CompletionStatsViewModel _DD_CompletionStats;
    private static SelectionStats _DD_SelectionStats;
    private static TableFieldToolTipViewModel _DD_TableFieldToolTipViewModel;
    private static AnnotationDialogContentViewModel _DD_AnnotationDialogContentViewModel;
    private static RichTextModel _DD_RichTextModel;
    private static AnnotationTemplateModel _DD_AnnotationModel;
    private static TaskPanelViewModel _DD_TaskPanelViewModel;
    private static TaskPanelFieldsTabViewModel _DD_TaskPanelFieldsTabViewModel;
    private static TaskPanelLayersTabViewModel _DD_TaskPanelLayersTabViewModel;
    private static TaskPanelFiltersTabViewModel _DD_TaskPanelFiltersTabViewModel;
    private static SceneViewModel _DD_SceneViewModel;
    private static SceneSettingsViewModel _DD_SceneSettingsViewModel;
    private static LayerSettingsViewModel _DD_LayerSettingsViewModel;
    private static TaskPanelSettingsTabViewModel _DD_TaskPanelSettingsTabViewModel;
    private static FieldWellHeightViewModel _DD_FieldWellHeightViewModel;
    private static TimeScrubberViewModel _DD_TimeViewModel;
    private static DateTimeEditorViewModel _DD_DateTimeEditorViewModel;
    private static TimeSettingsViewModel _DD_TimeSettingsViewModel;
    private static FieldWellTimeViewModel _DD_FieldWellTimeViewModel;
    private static ConfirmationDialogViewModel _DD_ConfirmationDialogViewModel;
    private static LabelDecoratorModel _DD_LabelDecoratorModel;
    private static LayerLegendDecoratorModel _DD_LayerLegendDecoratorModel;
    private static TimeDecoratorModel _DD_TimeDecoratorModel;
    private static AddLabelDialogViewModel _DD_AddLabelDialogViewModel;
    private static AddChartDialogViewModel _DD_AddChartDialogViewModel;
    private static EditTimeDecoratorDialogViewModel _DD_EditTimeDecoratorDialogViewModel;
    private static LayerLegendItemModel _DD_LayerLegendItemModel;
    private static GeocodingReportViewModel _DD_GeocodingReportViewModel;

    public static HostControlViewModel DD_HostWindowViewModel
    {
      get
      {
        return SampleData._DD_HostWindowViewModel;
      }
    }

    public static LayerViewModel DD_LayerViewModel
    {
      get
      {
        return SampleData._DD_LayerViewModel;
      }
    }

    public static LayerManagerViewModel DD_LayerManagerViewModel
    {
      get
      {
        return SampleData._DD_LayerManagerViewModel;
      }
    }

    public static FieldListPickerViewModel DD_FieldListPickerViewModel
    {
      get
      {
        return SampleData._DD_FieldListPickerViewModel;
      }
    }

    public static TableIslandViewModel DD_TableIslandViewModel
    {
      get
      {
        return SampleData._DD_TableIslandViewModel;
      }
    }

    public static TableViewModel DD_TableViewModel
    {
      get
      {
        return SampleData._DD_TableViewModel;
      }
    }

    public static FieldWellGeographyViewModel DD_FieldWellGeographyViewModel
    {
      get
      {
        return SampleData._DD_FieldWellGeographyViewModel;
      }
    }

    public static FieldWellVisualizationViewModel DD_FieldWellVisualizationViewModel
    {
      get
      {
        return SampleData._DD_FieldWellVisualizationViewModel;
      }
    }

    public static StatusBarViewModel DD_StatusBarViewModel
    {
      get
      {
        return SampleData._DD_StatusBarViewModel;
      }
    }

    public static CompletionStatsViewModel DD_CompletionStats
    {
      get
      {
        return SampleData._DD_CompletionStats;
      }
    }

    public static SelectionStats DD_SelectionStats
    {
      get
      {
        return SampleData._DD_SelectionStats;
      }
    }

    public static TableFieldToolTipViewModel DD_TableFieldToolTipViewModel
    {
      get
      {
        return SampleData._DD_TableFieldToolTipViewModel;
      }
    }

    public static AnnotationDialogContentViewModel DD_AnnotationDialogContentViewModel
    {
      get
      {
        return SampleData._DD_AnnotationDialogContentViewModel;
      }
    }

    public static RichTextModel DD_RichTextModel
    {
      get
      {
        return SampleData._DD_RichTextModel;
      }
    }

    public static AnnotationTemplateModel DD_AnnotationModel
    {
      get
      {
        return SampleData._DD_AnnotationModel;
      }
    }

    public static TaskPanelViewModel DD_TaskPanelViewModel
    {
      get
      {
        return SampleData._DD_TaskPanelViewModel;
      }
    }

    public static TaskPanelFieldsTabViewModel DD_TaskPanelFieldsTabViewModel
    {
      get
      {
        return SampleData._DD_TaskPanelFieldsTabViewModel;
      }
    }

    public static TaskPanelLayersTabViewModel DD_TaskPanelLayersTabViewModel
    {
      get
      {
        return SampleData._DD_TaskPanelLayersTabViewModel;
      }
    }

    public static TaskPanelFiltersTabViewModel DD_TaskPanelFiltersTabViewModel
    {
      get
      {
        return SampleData._DD_TaskPanelFiltersTabViewModel;
      }
    }

    public static SceneViewModel DD_SceneViewModel
    {
      get
      {
        return SampleData._DD_SceneViewModel;
      }
    }

    public static SceneSettingsViewModel DD_SceneSettingsViewModel
    {
      get
      {
        return SampleData._DD_SceneSettingsViewModel;
      }
    }

    public static LayerSettingsViewModel DD_LayerSettingsViewModel
    {
      get
      {
        return SampleData._DD_LayerSettingsViewModel;
      }
    }

    public static TaskPanelSettingsTabViewModel DD_TaskPanelSettingsTabViewModel
    {
      get
      {
        return SampleData._DD_TaskPanelSettingsTabViewModel;
      }
    }

    public static FieldWellHeightViewModel DD_FieldWellHeightViewModel
    {
      get
      {
        return SampleData._DD_FieldWellHeightViewModel;
      }
    }

    public static TimeScrubberViewModel DD_TimeViewModel
    {
      get
      {
        return SampleData._DD_TimeViewModel;
      }
    }

    public static DateTimeEditorViewModel DD_DateTimeEditorViewModel
    {
      get
      {
        return SampleData._DD_DateTimeEditorViewModel;
      }
    }

    public static TimeSettingsViewModel DD_TimeSettingsViewModel
    {
      get
      {
        return SampleData._DD_TimeSettingsViewModel;
      }
    }

    public static FieldWellTimeViewModel DD_FieldWellTimeViewModel
    {
      get
      {
        return SampleData._DD_FieldWellTimeViewModel;
      }
    }

    public static ConfirmationDialogViewModel DD_ConfirmationDialogViewModel
    {
      get
      {
        return SampleData._DD_ConfirmationDialogViewModel;
      }
    }

    public static LabelDecoratorModel DD_LabelDecoratorModel
    {
      get
      {
        return SampleData._DD_LabelDecoratorModel;
      }
    }

    public static LayerLegendDecoratorModel DD_LayerLegendDecoratorModel
    {
      get
      {
        return SampleData._DD_LayerLegendDecoratorModel;
      }
    }

    public static TimeDecoratorModel DD_TimeDecoratorModel
    {
      get
      {
        return SampleData._DD_TimeDecoratorModel;
      }
    }

    public static AddLabelDialogViewModel DD_AddLabelDialogViewModel
    {
      get
      {
        return SampleData._DD_AddLabelDialogViewModel;
      }
    }

    public static AddChartDialogViewModel DD_AddChartDialogViewModel
    {
      get
      {
        return SampleData._DD_AddChartDialogViewModel;
      }
    }

    public static EditTimeDecoratorDialogViewModel DD_EditTimeDecoratorDialogViewModel
    {
      get
      {
        return SampleData._DD_EditTimeDecoratorDialogViewModel;
      }
    }

    public static LayerLegendItemModel DD_LayerLegendItemModel
    {
      get
      {
        return SampleData._DD_LayerLegendItemModel;
      }
    }

    public static GeocodingReportViewModel DD_GeocodingReportViewModel
    {
      get
      {
        return SampleData._DD_GeocodingReportViewModel;
      }
    }

    static SampleData()
    {
      SampleData._DD_TableViewModel.Fields.Add(new TableFieldViewModel()
      {
        Name = "Zip"
      });
      SampleData._DD_TableViewModel.Fields.Add(new TableFieldViewModel()
      {
        Name = "State"
      });
      SampleData._DD_TableViewModel.Fields.Add(new TableFieldViewModel()
      {
        Name = "Value 1"
      });
      SampleData._DD_TableViewModel.Fields.Add(new TableFieldViewModel()
      {
        Name = "Value 2"
      });
      SampleData._DD_TableViewModel.Fields.Add(new TableFieldViewModel()
      {
        Name = "Category 1"
      });
      SampleData._DD_TableViewModel.IsEnabled = false;
      SampleData._DD_TableIslandViewModel.Tables.Add(SampleData._DD_TableViewModel);
      TableViewModel tableViewModel1 = new TableViewModel()
      {
        Name = "Table 3"
      };
      tableViewModel1.Fields.Add(new TableFieldViewModel()
      {
        Name = "Category 1"
      });
      tableViewModel1.Fields.Add(new TableFieldViewModel()
      {
        Name = "Value 2"
      });
      SampleData._DD_TableIslandViewModel.Tables.Add(tableViewModel1);
      SampleData._DD_FieldListPickerViewModel.TableIslandsForGeography.Add(SampleData._DD_TableIslandViewModel);
      SampleData._DD_FieldListPickerViewModel.TableIslandsForVisualization.Add(SampleData._DD_TableIslandViewModel);
      TableIslandViewModel tableIslandViewModel = new TableIslandViewModel()
      {
        IsEnabled = false
      };
      TableViewModel tableViewModel2 = new TableViewModel()
      {
        Name = "Range 1"
      };
      tableViewModel2.Fields.Add(new TableFieldViewModel()
      {
        Name = "Store Address"
      });
      tableViewModel2.Fields.Add(new TableFieldViewModel()
      {
        Name = "Store Manager"
      });
      tableViewModel2.Fields.Add(new TableFieldViewModel()
      {
        Name = "Store Name"
      });
      tableViewModel2.Fields.Add(new TableFieldViewModel()
      {
        Name = "Store Zip"
      });
      tableIslandViewModel.Tables.Add(tableViewModel2);
      SampleData._DD_FieldListPickerViewModel.TableIslandsForGeography.Add(tableIslandViewModel);
      SampleData._DD_FieldListPickerViewModel.TableIslandsForVisualization.Add(tableIslandViewModel);
      SampleData._DD_FieldListPickerViewModel.State = FieldListPickerState.ChooseVisField;
      SampleData._DD_LayerViewModel = new LayerViewModel()
      {
        FieldListPicker = SampleData._DD_FieldListPickerViewModel,
        Name = "Sample Layer Name"
      };
      SampleData._DD_HostWindowViewModel = new HostControlViewModel((VisualizationModel) null, (IHelpViewer) null, (BingMapResourceUri) null, (List<Color4F>) null);
      SampleData._DD_FieldWellGeographyViewModel = new FieldWellGeographyViewModel((IDialogServiceProvider) null);
      SampleData._DD_FieldWellGeographyViewModel.AddGeoField(new TableFieldViewModel()
      {
        Name = "Lat"
      });
      SampleData._DD_FieldWellGeographyViewModel.AddGeoField(new TableFieldViewModel()
      {
        Name = "Lon"
      });
      SampleData._DD_FieldWellVisualizationViewModel = new FieldWellVisualizationViewModel(RegionLayerShadingMode.Global)
      {
        SelectedVisualizationType = LayerType.BubbleChart
      };
      ObservableCollectionEx<FieldWellHeightViewModel> heightFields = SampleData._DD_FieldWellVisualizationViewModel.HeightFields;
      FieldWellHeightViewModel wellHeightViewModel1 = new FieldWellHeightViewModel(true);
      wellHeightViewModel1.TableField = new TableFieldViewModel()
      {
        Name = "population"
      };
      FieldWellHeightViewModel wellHeightViewModel2 = wellHeightViewModel1;
      heightFields.Add(wellHeightViewModel2);
      TableFieldViewModel tableFieldViewModel = new TableFieldViewModel()
      {
        Name = "Year",
        IsTimeField = true
      };
      NPCContainer<FieldWellTimeViewModel> selectedTimeField = SampleData._DD_FieldWellVisualizationViewModel.SelectedTimeField;
      FieldWellTimeViewModel wellTimeViewModel1 = new FieldWellTimeViewModel();
      wellTimeViewModel1.TableField = tableFieldViewModel;
      FieldWellTimeViewModel wellTimeViewModel2 = wellTimeViewModel1;
      selectedTimeField.Value = wellTimeViewModel2;
      NPCContainer<FieldWellCategoryViewModel> selectedCategory = SampleData._DD_FieldWellVisualizationViewModel.SelectedCategory;
      FieldWellCategoryViewModel categoryViewModel1 = new FieldWellCategoryViewModel();
      categoryViewModel1.TableField = tableFieldViewModel;
      FieldWellCategoryViewModel categoryViewModel2 = categoryViewModel1;
      selectedCategory.Value = categoryViewModel2;
      SampleData._DD_CompletionStats = new CompletionStatsViewModel(new CompletionStats()
      {
        Requested = 500,
        Completed = 370
      });
      SampleData._DD_SelectionStats = new SelectionStats();
      SampleData._DD_SelectionStats.UpdateWithValue(11.0 / 3.0);
      SampleData._DD_StatusBarViewModel = new StatusBarViewModel((ILayerManagerViewModel) null, (HostControlViewModel) null);
      SampleData._DD_StatusBarViewModel.CompletionStats = SampleData._DD_CompletionStats;
      SampleData._DD_StatusBarViewModel.SelectionStats = SampleData._DD_SelectionStats;
      SampleData._DD_TableFieldToolTipViewModel = new TableFieldToolTipViewModel();
      SampleData._DD_TableFieldToolTipViewModel.ToolTipProperties.Add(new Tuple<string, string>("Value", "15"));
      SampleData._DD_TableFieldToolTipViewModel.ToolTipProperties.Add(new Tuple<string, string>("City", "Seattle"));
      SampleData._DD_TableFieldToolTipViewModel.ToolTipProperties.Add(new Tuple<string, string>("State", "WA"));
      SampleData._DD_TableFieldToolTipViewModel.ToolTipProperties.Add(new Tuple<string, string>("Address", "1232938573 meowmeowmeowmeowmeowmeowmeow street"));
      SampleData._DD_TableFieldToolTipViewModel.ToolTipProperties.Add(new Tuple<string, string>("SOMEREALLYLONGTABLEFIELDCOLUMNLABEL", "1232938573"));
      SampleData._DD_RichTextModel = new RichTextModel()
      {
        FormatType = RichTextFormatType.Static,
        Text = "Custom Title",
        Color = Colors.Red,
        FontFamily = "Georgia",
        FontSize = 16,
        FontWeight = FontWeights.Bold.ToString()
      };
      SampleData._DD_AnnotationModel = new AnnotationTemplateModel()
      {
        Title = SampleData._DD_RichTextModel,
        Description = new RichTextModel()
        {
          FormatType = RichTextFormatType.Static,
          Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
          Color = Colors.SteelBlue,
          FontFamily = "Verdana",
          FontSize = 12,
          FontWeight = FontWeights.Thin.ToString(),
          FontStyle = FontStyles.Italic.ToString()
        },
        FieldFormat = new RichTextModel()
        {
          FormatType = RichTextFormatType.Template,
          TextTemplate = "{0}:{1}",
          Color = Colors.SaddleBrown,
          FontFamily = "Times New Roman",
          FontSize = 14,
          FontWeight = FontWeights.Bold.ToString()
        },
        DescriptionType = AnnotationDescriptionType.Bound
      };
      SampleData._DD_LabelDecoratorModel = new LabelDecoratorModel()
      {
        Title = new RichTextModel()
        {
          FormatType = RichTextFormatType.Static,
          Text = "Lorem ipsum dolor",
          Color = Colors.DarkGray,
          FontFamily = "Segoe UI",
          FontSize = 26,
          FontWeight = FontWeights.Thin.ToString()
        },
        Description = new RichTextModel()
        {
          FormatType = RichTextFormatType.Static,
          Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
          Color = Colors.DarkGray,
          FontFamily = "Verdana",
          FontSize = 12,
          FontWeight = FontWeights.Thin.ToString()
        }
      };
      SampleData._DD_AddLabelDialogViewModel = new AddLabelDialogViewModel()
      {
        Label = SampleData._DD_LabelDecoratorModel
      };
      SampleData._DD_AnnotationModel.ColumnAggregationFunctions.Add(new AggregationFunction?());
      SampleData._DD_AnnotationModel.NamesOfColumnsToDisplay.Add("City");
      SampleData._DD_AnnotationModel.ColumnAggregationFunctions.Add(new AggregationFunction?());
      SampleData._DD_AnnotationModel.NamesOfColumnsToDisplay.Add("State");
      SampleData._DD_AnnotationModel.ColumnAggregationFunctions.Add(new AggregationFunction?());
      SampleData._DD_AnnotationModel.NamesOfColumnsToDisplay.Add("Zip");
      SampleData._DD_AnnotationModel.ColumnAggregationFunctions.Add(new AggregationFunction?(AggregationFunction.Sum));
      SampleData._DD_AnnotationModel.NamesOfColumnsToDisplay.Add("Population");
      SampleData._DD_AnnotationModel.ColumnAggregationFunctions.Add(new AggregationFunction?());
      SampleData._DD_AnnotationModel.NamesOfColumnsToDisplay.Add("Year");
      SampleData._DD_AnnotationDialogContentViewModel = new AnnotationDialogContentViewModel((AnnotationTemplateModel) null)
      {
        Model = SampleData._DD_AnnotationModel,
        SelectedData = new DataRowModel((List<Tuple<AggregationFunction?, string, object>>) null, false)
        {
          Fields = {
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(), "City", (object) "Seattle"),
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(), "State", (object) "WA"),
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(), "Zip", (object) 98052),
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(), "Country", (object) "USA"),
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(AggregationFunction.None), "Population", (object) 608660),
            new Tuple<AggregationFunction?, string, object>(new AggregationFunction?(), "Year", (object) 2010)
          }
        },
        InstructionText = "Instruction Text"
      };
      SampleData._DD_TaskPanelFieldsTabViewModel = new TaskPanelFieldsTabViewModel((ILayerManagerViewModel) new LayerManagerViewModel());
      SampleData._DD_TaskPanelFieldsTabViewModel.Model.Layers.Add(SampleData._DD_LayerViewModel);
      SampleData._DD_TaskPanelFieldsTabViewModel.Model.SelectedLayer = SampleData._DD_LayerViewModel;
      FieldWellHeightViewModel wellHeightViewModel3 = new FieldWellHeightViewModel(true);
      wellHeightViewModel3.TableField = new TableFieldViewModel()
      {
        Name = "population"
      };
      wellHeightViewModel3.AggregationFunction = AggregationFunction.Average;
      SampleData._DD_FieldWellHeightViewModel = wellHeightViewModel3;
      SampleData._DD_TaskPanelLayersTabViewModel = new TaskPanelLayersTabViewModel((ILayerManagerViewModel) null);
      SampleData._DD_TaskPanelLayersTabViewModel.Layers.Add(new LayerViewModel()
      {
        Name = "Layer Group 1"
      });
      SampleData._DD_TaskPanelLayersTabViewModel.Layers.Add(new LayerViewModel()
      {
        Name = "Layer Group 2"
      });
      SampleData._DD_TaskPanelLayersTabViewModel.Layers.Add(new LayerViewModel()
      {
        Name = "Layer Group 3",
        Visible = false
      });
      SampleData._DD_TaskPanelFiltersTabViewModel = new TaskPanelFiltersTabViewModel((ILayerManagerViewModel) null);
      SampleData._DD_SceneViewModel = new SceneViewModel(new Scene()
      {
        Name = "Yet another blue scene with a really really really really really long name!"
      }, 12, (GlobeViewModel) null);
      SampleData._DD_LayerManagerViewModel = new LayerManagerViewModel();
      SampleData._DD_LayerManagerViewModel.Layers.Add(SampleData._DD_LayerViewModel);
      SampleData._DD_TimeViewModel = new TimeScrubberViewModel(SampleData._DD_HostWindowViewModel, (ITimeController) null, new LayerManagerViewModel(), (Action) (() => {}));
      SampleData._DD_TimeSettingsViewModel = new TimeSettingsViewModel();
      SampleData._DD_SceneSettingsViewModel = new SceneSettingsViewModel(SampleData._DD_HostWindowViewModel, SampleData._DD_TimeSettingsViewModel, SampleData._DD_TimeViewModel);
      SampleData._DD_SceneSettingsViewModel.ParentScene = SampleData._DD_SceneViewModel;
      SampleData._DD_TaskPanelViewModel = new TaskPanelViewModel((ILayerManagerViewModel) null, SampleData._DD_SceneSettingsViewModel);
      SampleData._DD_TaskPanelViewModel.LayersTab = SampleData._DD_TaskPanelLayersTabViewModel;
      SampleData._DD_TaskPanelViewModel.FieldsTab = SampleData._DD_TaskPanelFieldsTabViewModel;
      SampleData._DD_LayerSettingsViewModel = new LayerSettingsViewModel((ILayerManagerViewModel) null, (IThemeService) new SampleData.MockThemeService(), (List<Color4F>) null);
      SampleData._DD_TaskPanelSettingsTabViewModel = new TaskPanelSettingsTabViewModel(SampleData._DD_LayerSettingsViewModel, SampleData._DD_SceneSettingsViewModel);
      SampleData._DD_TaskPanelViewModel.SettingsTab = SampleData._DD_TaskPanelSettingsTabViewModel;
      SampleData._DD_TaskPanelViewModel.FiltersTab = SampleData._DD_TaskPanelFiltersTabViewModel;
      SampleData._DD_TaskPanelViewModel.ChangeCurrentSettings(TaskPanelSettingsSubhead.SceneSettings);
      FieldWellTimeViewModel wellTimeViewModel3 = new FieldWellTimeViewModel();
      wellTimeViewModel3.TableField = new TableFieldViewModel()
      {
        Name = "Table Name"
      };
      wellTimeViewModel3.TimeChunk = TimeChunkPeriod.Year;
      SampleData._DD_FieldWellTimeViewModel = wellTimeViewModel3;
      SampleData._DD_DateTimeEditorViewModel = new DateTimeEditorViewModel()
      {
        CurrentCultureUsesTwentyFourHourFormat = false
      };
      ConfirmationDialogViewModel confirmationDialogViewModel = new ConfirmationDialogViewModel();
      confirmationDialogViewModel.Title = "Delete this page?";
      confirmationDialogViewModel.Description = "This action cannot be undone.  Make a backup of this file before performing this operation.";
      SampleData._DD_ConfirmationDialogViewModel = confirmationDialogViewModel;
      SampleData._DD_ConfirmationDialogViewModel.Commands.Add(new DelegatedCommand((Action) (() => {}))
      {
        Name = "Save"
      });
      SampleData._DD_ConfirmationDialogViewModel.Commands.Add(new DelegatedCommand((Action) (() => {}))
      {
        Name = "Overwrite"
      });
      SampleData._DD_ConfirmationDialogViewModel.Commands.Add(new DelegatedCommand((Action) (() => {}))
      {
        Name = "Cancel"
      });
      SampleData._DD_LayerLegendDecoratorModel = new LayerLegendDecoratorModel();
      SampleData._DD_LayerLegendDecoratorModel.LegendItems.Add(new LayerLegendItemModel("Meow", Colors.Azure, new double?(), new double?(), false, false));
      SampleData._DD_LayerLegendDecoratorModel.LegendItems.Add(new LayerLegendItemModel("Woof", Colors.BlanchedAlmond, new double?(), new double?(), false, false));
      SampleData._DD_LayerLegendDecoratorModel.LegendItems.Add(new LayerLegendItemModel("Foo", Colors.Crimson, new double?(), new double?(), false, false));
      SampleData._DD_LayerLegendItemModel = new LayerLegendItemModel("Cats", Colors.Aqua, new double?(), new double?(), false, false);
      SampleData._DD_TimeDecoratorModel = new TimeDecoratorModel();
      SampleData._DD_TimeDecoratorModel.Time = DateTime.Now;
      SampleData._DD_GeocodingReportViewModel = new GeocodingReportViewModel(new List<GeoAmbiguity>()
      {
        new GeoAmbiguity((string) null, (GeoField) null, "1 Microsoft Way", "Redmond", (string) null, "WA", "98052", (string) null, (string) null, GeoAmbiguity.Resolution.NoMatch, 0, (List<GeoResolution>) null),
        new GeoAmbiguity((string) null, (GeoField) null, "1000 Jefferson Drive Southwest", "Washington", (string) null, "DC", "20560", (string) null, (string) null, GeoAmbiguity.Resolution.SingleMatchHighConf, 0, new List<GeoResolution>()
        {
          new GeoResolution()
          {
            FormattedAddress = "1000 Jefferson Drive Southwest, Washington DC 20560"
          }
        })
      }, 1f)
      {
        Confidence = "44%",
        LayerName = "Layer 1"
      };
      SampleData._DD_EditTimeDecoratorDialogViewModel = new EditTimeDecoratorDialogViewModel(SampleData._DD_TimeDecoratorModel);
      SampleData._DD_AddChartDialogViewModel = new AddChartDialogViewModel();
    }

    public class MockThemeService : IThemeService
    {
      private int ThemeIndex;

      private static Color4F[] DummyColor
      {
        get
        {
          return Enumerable.ToArray<Color4F>(Enumerable.Select<System.Windows.Media.Color, Color4F>((IEnumerable<System.Windows.Media.Color>) new System.Windows.Media.Color[6]
          {
            Colors.Red,
            Colors.Blue,
            Colors.Brown,
            Colors.Black,
            Colors.Orange,
            Colors.Green
          }, (Func<System.Windows.Media.Color, Color4F>) (c => ColorExtensions.ToColor4F(c))));
        }
      }

      private static Color4F[] DummyColor2
      {
        get
        {
          return Enumerable.ToArray<Color4F>(Enumerable.Select<System.Windows.Media.Color, Color4F>((IEnumerable<System.Windows.Media.Color>) new System.Windows.Media.Color[6]
          {
            Colors.Blue,
            Colors.Green,
            Colors.Purple,
            Colors.Red,
            Colors.Orange,
            Colors.BlueViolet
          }, (Func<System.Windows.Media.Color, Color4F>) (c => ColorExtensions.ToColor4F(c))));
        }
      }

      public IEnumerable<Color4F> ThemeColors
      {
        get
        {
          switch (this.ThemeIndex % 3)
          {
            case 0:
              return Enumerable.SelectMany<Color4F[], Color4F>((IEnumerable<Color4F[]>) new Color4F[3][]
              {
                SampleData.MockThemeService.DummyColor,
                SampleData.MockThemeService.DummyColor,
                SampleData.MockThemeService.DummyColor
              }, (Func<Color4F[], IEnumerable<Color4F>>) (x => (IEnumerable<Color4F>) x));
            case 1:
              return Enumerable.SelectMany<Color4F[], Color4F>((IEnumerable<Color4F[]>) new Color4F[3][]
              {
                SampleData.MockThemeService.DummyColor2,
                SampleData.MockThemeService.DummyColor2,
                SampleData.MockThemeService.DummyColor2
              }, (Func<Color4F[], IEnumerable<Color4F>>) (x => (IEnumerable<Color4F>) x));
            default:
              return Enumerable.SelectMany<Color4F[], Color4F>((IEnumerable<Color4F[]>) new Color4F[3][]
              {
                SampleData.MockThemeService.DummyColor2,
                SampleData.MockThemeService.DummyColor2,
                SampleData.MockThemeService.DummyColor2
              }, (Func<Color4F[], IEnumerable<Color4F>>) (x => (IEnumerable<Color4F>) x));
          }
        }
      }

      public event Action OnThemeChanged;

      internal void NextTheme()
      {
        ++this.ThemeIndex;
        this.OnThemeChanged();
      }
    }

    public class MockTimeController : ITimeController, INotifyPropertyChanged
    {
      private TimeSpan _visualDuration;
      private bool _visualTimeEnabled;
      private bool _looping;
      private DateTime _currentVisualTime;

      public string PropertyVisualTimeEnabled
      {
        get
        {
          return "VisualTimeEnabled";
        }
      }

      public string PropertyLooping
      {
        get
        {
          return "Looping";
        }
      }

      public string PropertyCurrentVisualTime
      {
        get
        {
          return "CurrentVisualTime";
        }
      }

      public string PropertyDuration
      {
        get
        {
          return "Duration";
        }
      }

      public TimeSpan Duration
      {
        get
        {
          return this._visualDuration;
        }
        set
        {
          this._visualDuration = value;
        }
      }

      public bool VisualTimeEnabled
      {
        get
        {
          return this._visualTimeEnabled;
        }
        set
        {
          this._visualTimeEnabled = value;
        }
      }

      public bool Looping
      {
        get
        {
          return this._looping;
        }
        set
        {
          this._looping = value;
        }
      }

      public DateTime CurrentVisualTime
      {
        get
        {
          return this._currentVisualTime;
        }
        set
        {
          this._currentVisualTime = value;
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public void SetVisualTimeRange(DateTime startTime, DateTime endTime, bool unionWithCurrentRange)
      {
      }
    }

    public class MockLayerManager : LayerManager
    {
      private DateTime? _playFromTime;
      private DateTime? _playToTime;

      public override DateTime? MinTime
      {
        get
        {
          return new DateTime?(new DateTime(1960, 1, 1));
        }
      }

      public override DateTime? MaxTime
      {
        get
        {
          return new DateTime?(new DateTime(2010, 12, 31));
        }
      }

      public override DateTime? PlayFromTime
      {
        get
        {
          if (this._playFromTime.HasValue)
            return this._playFromTime;
          else
            return this.MinTime;
        }
        set
        {
          this._playFromTime = value;
        }
      }

      public override DateTime? PlayToTime
      {
        get
        {
          if (this._playToTime.HasValue)
            return this._playToTime;
          else
            return this.MaxTime;
        }
        set
        {
          this._playToTime = value;
        }
      }

      public MockLayerManager()
        : base(new VisualizationModel((IDataSourceFactory) new SampleData.MockDataSourceFactory(), new GeospatialDataProviders()
        {
          LatLonProvider = (ILatLonProvider) new SampleData.MockLatLonProvider()
        }, (ITourPersist) new SampleData.MockTourPersist(), (IModelWrapper) new SampleData.MockModelWrapper()), (IDataSourceFactory) new SampleData.MockDataSourceFactory())
      {
      }
    }

    public class MockDataSourceFactory : IDataSourceFactory
    {
      public DataSource CreateDataSource(string name)
      {
        throw new NotImplementedException();
      }
    }

    public class MockTourPersist : ITourPersist
    {
      public void PersistTour(Tour tour)
      {
        throw new NotImplementedException();
      }
    }

    public class MockLatLonProvider : ILatLonProvider
    {
      public Action<Exception> OnInternalError { get; set; }

      public CultureInfo ModelCulture { get; set; }

      public void GetLatLon(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities)
      {
        throw new NotImplementedException();
      }

      public void GetLatLonAsync(GeoField geoField, int firstGeoCol, int count, Func<int, int, string> geoValuesAccessor, Func<int, int> geoRowsAccessor, CancellationToken cancellationToken, double[] lat, double[] lon, GeoResolutionBorder[] boundingBox, GeoAmbiguity[] ambiguities, CompletionStats stats, Action<object, int, int, int> latLonResolvedCallback = null, Action<object> completionCallback = null, object context = null)
      {
        throw new NotImplementedException();
      }

      public bool GetLatLon(string geoQuery, CancellationToken cancellationToken, out double lat, out double lon, out GeoResolutionBorder boundingBox, out GeoAmbiguity ambiguity)
      {
        throw new NotImplementedException();
      }

      public void GetLatLonAsync(string geoQuery, CancellationToken cancellationToken, Action<object, bool, double, double, GeoResolutionBorder, GeoAmbiguity> completionCallback = null, object context = null)
      {
        throw new NotImplementedException();
      }
    }

    public class MockModelWrapper : IModelWrapper
    {
      public bool ConnectionsDisabled
      {
        get
        {
          return true;
        }
      }

      public List<TableIsland> GetTableMetadata()
      {
        throw new NotImplementedException();
      }

      ModelMetadata IModelWrapper.GetTableMetadata()
      {
        throw new NotImplementedException();
      }

      void IModelWrapper.RefreshAll()
      {
      }
    }
  }
}
