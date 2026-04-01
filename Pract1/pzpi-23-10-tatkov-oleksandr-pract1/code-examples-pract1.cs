Запит ШІ для рефакторингу классу за патерном:

Перепиши існуючий клас за патерном Factory.

Винеси реалізації конкретних сабтайпів у окремі класи з назвою у форматі {ClassName}Parser.

Кожен парсер має реалізовувати спільний інтерфейс  і відповідати за обробку лише свого типу.

У композері (реєстраторі) реалізуй реєстрацію цих класів у словнику або контейнері по шаблону, щоб можна було легко додавати нові парсери без зміни основного коду.

Весь код має бути розміщений у неймспейсі Application.SubTypeParsers.GuiTypes.

Програмний код
1) BaseGuiParser та IGuiElementParser

namespace Application.SubTypeParsers
{
    public interface IGuiElementParser
    {
        string ElementType { get; }
        IGui Parse(Bracket bracket);
    }
}

namespace Application.SubTypeParsers
{
    public abstract class BaseGuiParser : IGuiElementParser
    {
        public abstract string ElementType { get; }

        public abstract IGui Parse(Bracket bracket);

        // Допоміжні методи, доступні всім парсерам
        protected void ParseCommonProperties(IGui entity, Bracket br)
        {
            foreach (var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        entity.Id = new Identifier(vr.Value.ToString());
                        break;
                    case "orientation":
                        if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString().SnakeToPascal(), true, out var orient))
                            entity.Orientation = orient; // потрібно привести до конкретного типу, якщо потрібно
                        break;
                    case "alwaystransparent":
                        if (entity is IGui baseEntity)
                            baseEntity.AlwaysTransparent = vr.Value.ToBool();
                        break;
                    case "pdx_tooltip":
                        if (entity is IGui baseEntity2)
                            baseEntity2.PdxTooltip = vr.Value.ToString();
                        break;
                    case "pdx_tooltip_delayed":
                        if (entity is IGui baseEntity3)
                            baseEntity3.PdxTooltipDelayed = vr.Value.ToString();
                        break;
                }
            }
        }

        #region Helper Methods
        protected static Point GetPosFromBracket(Bracket br)
        {
            int x = 0, y = 0;
            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "x":
                        x = vr.Value.ToInt();
                        break;
                    case "y":
                        y = vr.Value.ToInt();
                        break;
                }
            }
            return new Point(x, y);
        }

        protected static SizeDefinition GetSizeFromBracket(Bracket br)
        {
            int? width = null, height = null;
            int? minWidthPercent = null, minHeightPercent = null;
            int? maxWidthPercent = null, maxHeightPercent = null;

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "width":
                        width = vr.Value.ToInt();
                        break;
                    case "height":
                        height = vr.Value.ToInt();
                        break;
                    case "min":
                        if (vr.Value is List<Var> minVars)
                        {
                            foreach (var subVr in minVars)
                            {
                                switch (subVr.Name.ToLower())
                                {
                                    case "width":
                                    case "x":
                                        minWidthPercent = subVr.Value.ToString().Replace("%", "").ToInt();
                                        break;
                                    case "height":
                                    case "y":
                                        minHeightPercent = subVr.Value.ToString().Replace("%", "").ToInt();
                                        break;
                                }
                            }
                        }
                        break;
                    case "max":
                        if (vr.Value is List<Var> maxVars)
                        {
                            foreach (var subVr in maxVars)
                            {
                                switch (subVr.Name.ToLower())
                                {
                                    case "width":
                                    case "x":
                                        maxWidthPercent = subVr.Value.ToString().Replace("%", "").ToInt();
                                        break;
                                    case "height":
                                    case "y":
                                        maxHeightPercent = subVr.Value.ToString().Replace("%", "").ToInt();
                                        break;
                                }
                            }
                        }
                        break;
                }
            }

            return new SizeDefinition()
            {
                Width = width,
                Height = height,
                MinWidthPercent = minWidthPercent,
                MinHeightPercent = minHeightPercent,
                MaxWidthPercent = maxWidthPercent,
                MaxHeightPercent = maxHeightPercent
            };
        }

        protected static GuiBackgroundDefenition ParseBackground(Bracket br)
        {
            var background = new GuiBackgroundDefenition();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "name":
                        background.Name = vr.Value.ToString();
                        break;
                    case "spritetype":
                        background.SpriteType = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities).FirstOrDefault(e => e.Id.ToString() == vr.Value.ToString()) as SpriteType;
                        break;
                    case "quadtexturesprite":
                        background.QuadTextureSprite = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities).FirstOrDefault(e => e.Id.ToString() == vr.Value.ToString()) as CorneredTileSpriteType;
                        break;
                }
            }

            return background;
        }
        protected void ParseCommon(IGui entity, Bracket br)
        {
            if (entity is null) return;

            foreach (var vr in br.SubVars)
            {
                if (entity is IGui guiEntity)
                {
                    switch (vr.Name.ToLower())
                    {
                        case "name":
                            entity.Id = new Identifier(vr.Value.ToString());
                            break;
                        case "orientation":
                            if (Enum.TryParse<GuiOrientationType>(vr.Value.ToString().SnakeToPascal(), true, out var orient))
                                guiEntity.Orientation = orient;
                            break;
                        case "alwaystransparent":
                            guiEntity.AlwaysTransparent = vr.Value.ToBool();
                            break;
                        case "pdx_tooltip":
                            guiEntity.PdxTooltip = vr.Value.ToString();
                            break;
                        case "pdx_tooltip_delayed":
                            guiEntity.PdxTooltipDelayed = vr.Value.ToString();
                            break;
                    }
                }
            }
        }

        protected static GuiMarginDefenition ParseMargin(Bracket br)
        {
            var margin = new GuiMarginDefenition();

            foreach (Var vr in br.SubVars)
            {
                switch (vr.Name.ToLower())
                {
                    case "top":
                        margin.Top = vr.Value.ToInt();
                        break;
                    case "bottom":
                        margin.Bottom = vr.Value.ToInt();
                        break;
                    case "left":
                        margin.Left = vr.Value.ToInt();
                        break;
                    case "right":
                        margin.Right = vr.Value.ToInt();
                        break;
                }
            }

            return margin;
        }
        #endregion
    }
}

2) ContainerWindowParser.cs, IconParser.cs, ButtonParser.cs та інші конкретні парсери

public class SmoothListboxParser : BaseGuiParser
{
    public override string ElementType => "smoothlistboxtype";

    public override IGui Parse(Bracket br)
    {
        var listbox = new SmoothListboxType();
        ParseCommon(listbox, br);

        foreach (var vr in br.SubVars)
        {
            switch (vr.Name.ToLower())
            {
                case "spacing": listbox.Spacing = vr.Value.ToInt(); break;
                case "horizontal": listbox.Horizontal = vr.Value.ToBool(); break;
                case "bordersize": listbox.BorderSize = vr.Value.ToInt(); break;
                case "background": listbox.Background = vr.Value.ToString(); break;
                case "scrollbartype":
                    listbox.ScrollbarType = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                    break;
            }
        }

        foreach (var subBr in br.SubBrackets)
        {
            switch (subBr.Name.ToLower())
            {
                case "position": listbox.Position = GetPosFromBracket(subBr); break;
                case "size": listbox.Size = GetSizeFromBracket(subBr); break;
            }
        }

        return listbox;
    }
}

public class ScrollbarParser : BaseGuiParser
{
    public override string ElementType => "scrollbartype";

    public override IGui Parse(Bracket br)
    {
        var scrollbar = new ScrollbarType();
        ParseCommon(scrollbar, br);

        foreach (var vr in br.SubVars)
        {
            switch (vr.Name.ToLower())
            {
                case "priority": scrollbar.Priority = vr.Value.ToInt(); break;
                case "maxvalue": scrollbar.MaxValue = vr.Value.ToInt(); break;
                case "minvalue": scrollbar.MinValue = vr.Value.ToInt(); break;
                case "stepsize": scrollbar.StepSize = vr.Value.ToInt(); break;
                case "startvalue": scrollbar.StartValue = vr.Value.ToInt(); break;
                case "horizontal": scrollbar.Horizontal = vr.Value.ToBool(); break;
            }
        }

        foreach (var subBr in br.SubBrackets)
        {
            switch (subBr.Name.ToLower())
            {
                case "position":
                    scrollbar.Position = GetPosFromBracket(subBr);
                    break;
                case "size":
                    scrollbar.Size = GetSizeFromBracket(subBr);
                    break;
                case "bordersize":
                    scrollbar.BorderSize = GetSizeFromBracket(subBr);
                    break;
            }
        }

        ParseScrollBarButtons(scrollbar, br);
        return scrollbar;
    }

    private void ParseScrollBarButtons(ScrollbarType scrollbar, Bracket br)
    {
        string sliderName = "", trackName = "", leftButtonName = "", rightButtonName = "";

        foreach (var vr in br.SubVars.Where(v =>
            v.Name.ToLower() is "slider" or "track" or "leftbutton" or "rightbutton"))
        {
            switch (vr.Name.ToLower())
            {
                case "slider": sliderName = vr.Value.ToString(); break;
                case "track": trackName = vr.Value.ToString(); break;
                case "leftbutton": leftButtonName = vr.Value.ToString(); break;
                case "rightbutton": rightButtonName = vr.Value.ToString(); break;
            }
        }

        foreach (var subBr in br.SubBrackets.Where(b => b.Name.ToLower() == "guibuttontype"))
        {
            var button = new ButtonParser().Parse(subBr) as ButtonType;
            if (button == null) continue;

            if (!string.IsNullOrEmpty(sliderName) && button.Id?.ToString() == sliderName)
                scrollbar.Slider = button;
            else if (!string.IsNullOrEmpty(trackName) && button.Id?.ToString() == trackName)
                scrollbar.Track = button;
            else if (!string.IsNullOrEmpty(leftButtonName) && button.Id?.ToString() == leftButtonName)
                scrollbar.LeftButton = button;
            else if (!string.IsNullOrEmpty(rightButtonName) && button.Id?.ToString() == rightButtonName)
                scrollbar.RightButton = button;
        }
    }
}

public class OverlappingElementsBoxParser : BaseGuiParser
{
    public override string ElementType => "overlappingelementsboxtype";

    public override IGui Parse(Bracket br)
    {
        var box = new OverlappingElementsBoxType();
        ParseCommon(box, br);

        foreach (var vr in br.SubVars)
        {
            switch (vr.Name.ToLower())
            {
                case "spacing": box.Spacing = vr.Value.ToInt(); break;
                case "horizontal": box.Horizontal = vr.Value.ToBool(); break;
                case "bordersize": box.BorderSize = vr.Value.ToInt(); break;
                case "texturefile": box.TextureFile = vr.Value.ToString(); break;
            }
        }

        foreach (var subBr in br.SubBrackets)
        {
            switch (subBr.Name.ToLower())
            {
                case "position": box.Position = GetPosFromBracket(subBr); break;
                case "size": box.Size = GetSizeFromBracket(subBr); break;
            }
        }

        return box;
    }
}

public class ListboxParser : BaseGuiParser
 {
     public override string ElementType => "listboxtype";

     public override IGui Parse(Bracket br)
     {
         var listbox = new ListboxType();
         ParseCommon(listbox, br);

         foreach (var vr in br.SubVars)
         {
             switch (vr.Name.ToLower())
             {
                 case "name":
                     listbox.Id = new Identifier(vr.Value.ToString());
                     break;
                 case "spacing": listbox.Spacing = vr.Value.ToInt(); break;
                 case "horizontal": listbox.Horizontal = vr.Value.ToBool(); break;
                 case "bordersize": listbox.BorderSize = vr.Value.ToInt(); break;
                 case "background": listbox.Background = vr.Value.ToString(); break;
                 case "scrollbartype":
                     listbox.ScrollbarType = ModDataStorage.Mod.FindGuiType(vr.Value.ToString()) as ScrollbarType;
                     break;
             }
         }

         foreach (var subBr in br.SubBrackets)
         {
             switch (subBr.Name.ToLower())
             {
                 case "position": listbox.Position = GetPosFromBracket(subBr); break;
                 case "size": listbox.Size = GetSizeFromBracket(subBr); break;
             }
         }

         return listbox;
     }
 }
public class IconParser : BaseGuiParser
{
    public override string ElementType => "icontype";

    public override IGui Parse(Bracket br)
    {
        var icon = new IconType();
        ParseCommon(icon, br);

        foreach (var vr in br.SubVars)
        {
            switch (vr.Name.ToLower())
            {
                case "name":
                    icon.Id = new Identifier(vr.Value.ToString());
                    break;
                case "spritetype":
                    icon.SpriteType = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities)
                        .FirstOrDefault(d => d.Id.ToString() == vr.Value.ToString()) as SpriteType
                        ?? new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
                    break;
                case "quadtexturesprite":
                    icon.QuadTextureSprite = ModDataStorage.Mod.Gfxes.SelectMany(e => e.Entities)
                        .FirstOrDefault(d => d.Id.ToString() == vr.Value.ToString()) as CorneredTileSpriteType
                        ?? new CorneredTileSpriteType();
                    break;
                case "centerposition": icon.CenterPosition = vr.Value.ToBool(); break;
                case "frame": icon.Frame = vr.Value.ToInt(); break;
                case "hint_tag": icon.HintTag = vr.Value.ToString(); break;
            }
        }

        foreach (var subBr in br.SubBrackets)
        {
            if (subBr.Name.ToLower() == "position")
                icon.Position = GetPosFromBracket(subBr);
        }

        return icon;
    }
}

3) GuiComposer.cs

namespace Application.Composers
{
    public static class GuiComposer
    {
        private static readonly Dictionary<string, IGuiElementParser> _parsers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "containerwindowtype", new ContainerWindowParser() },
            { "instanttextboxtype", new InstantTextBoxParser() },
            { "icontype", new IconParser() },
            { "scrollbartype", new ScrollbarParser() },
            { "buttontype", new ButtonParser() },
            { "checkboxtype", new CheckboxParser() },
            { "listboxtype", new ListboxParser() },
            { "smoothlistboxtype", new SmoothListboxParser() },
            { "overlappingelementsboxtype", new OverlappingElementsBoxParser() },
            { "editboxtype", new EditBoxParser() }
        };
        public static List<GuiFile<IGui>> Parse()
        {
            List<GuiFile<IGui>> result = new List<GuiFile<IGui>>();
            string[] guidirs =
            {
                ModPathes.InterfacePath,
                GamePathes.InterfacePath,
            };

            List<string> seenModFiles = new List<string>();

            foreach (var dir in guidirs)
            {
                if (!Directory.Exists(dir)) continue;
                var files = Directory.GetFiles(dir, "*.gui", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        var guiFile = ParseFile(file);

                        if (dir == ModPathes.InterfacePath)
                        {
                            seenModFiles.Add(guiFile.FileName);
                        }
                        else
                        {
                            if (seenModFiles.Contains(guiFile.FileName))
                            {
                                continue;
                            }
                        }

                        result.AddSafe(guiFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.AddDbgLog(StaticLocalisation.GetString("DbgLog.FailedToParseGuiFile", file, ex));
                    }
                }
            }
            Logger.AddLog(StaticLocalisation.GetString("Log.ParsedGuiFilesCount", result.Count));
            return result;
        }
        public static GuiFile<IGui> ParseFile(string filePath)
        {
            FuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as FuncFile;
            GuiFile<IGui> res = new GuiFile<IGui> { FileFullPath = filePath };

            foreach (Bracket guiTypesBr in file.Brackets.Where(b => b.Name == "guiTypes"))
            {
                foreach (Bracket br in guiTypesBr.SubBrackets)
                {
                    IGui entity = ParseGuiElement(br);
                    if (entity != null)
                        res.Entities.Add(entity);
                }
            }
            return res;
        }

        /// <summary>
        /// Универсальный метод парсинга GUI элементов
        /// </summary>
        private static IGui ParseGuiElement(Bracket br)
        {
            string typeName = br.Name.ToLower();

            if (_parsers.TryGetValue(typeName, out var parser))
            {
                try
                {
                    return parser.Parse(br);
                }
                catch (Exception ex)
                {
                    Logger.AddDbgLog($"Помилка парсингу {typeName}: {ex.Message}");
                    return null;
                }
            }
            Logger.AddDbgLog(StaticLocalisation.GetString("DbgLog.UnknownGuiElementType", br.Name));
            return null;
        }
    }
}
