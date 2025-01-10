using Origins.Systems;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Origins.Gui
{

    /// <summary>
    /// HUD Element for ItemSeedAnalyzer
    /// </summary>
    public class HudSeedAnalyzer : HudElement
    {
        private Block currentBlock;

        private int currentSelectionIndex;

        private Entity currentEntity;

        private BlockPos currentPos;

        private string title;

        private string detail;

        private GuiComposer composer;

        //HudElementBlockAndEntityInfo
        //HudElementCoordinates

        const string AnalysisPlaceholderTitle = "Analysis: Placeholder";
        const string AnalysisPlaceholderDetail = "a: 0\nb: 0\nc: 0\nStatbar Test";

        public HudSeedAnalyzer(ICoreClientAPI capi) : base(capi)
        {
            ComposeSeedAnalysisHUD();
        }

        private void ComposeSeedAnalysisHUD()
        {

            string newTitle = "";
            string newDetail = "";

            if (currentBlock != null)
            {
                if (currentBlock.Code == null)
                {
                    newTitle = "";
                    newDetail = "";
                }

                // TODO (chris): we only want blocks that can display seed genetics info
                // if (currentBlock.CollectibleBehaviors.Contains(BehaviorSeedGenetics))
                if (currentBlock != null)
                {
                    // TODO (chris): can I add/change BlockBehavior to append to *BlockInfo(...)?
                    newTitle = currentBlock.GetPlacedBlockName(capi.World, currentPos);
                    // newDetail = currentBlock.GetPlacedBlockInfo(capi.World, currentPos, capi.World.Player);

                    if (newDetail == null)
                    {
                        newDetail = "";
                    }

                    if (newTitle == null)
                    {
                        newTitle = "Unknown";
                    }
                }
            }

            if (currentEntity != null)
            {
                // TODO (chris): we only want entities that can display seed genetics info
                // if (currentEntity.CollectibleBehaviors.Contains(BehaviorSeedGenetics))
                {
                    newTitle = currentEntity.GetName();
                    // newDetail = currentEntity.GetInfoText();

                    if (newDetail == null)
                    {
                        newDetail = "";
                    }

                    if (newTitle == null)
                    {
                        newTitle = "Unknown Entity code " + currentEntity.Code;
                    }
                }
            }

            // BUG (chris): hardcoded values
            newTitle = AnalysisPlaceholderTitle;
            newDetail = AnalysisPlaceholderDetail;

            // need to update HUD
            if (!(title == newTitle) || !(detail == newDetail))
            {
                title = newTitle;
                detail = newDetail;

                ElementBounds titleBounds = ElementBounds.Fixed(EnumDialogArea.None, 0.0, 0.0, 300, 24.0);
                ElementBounds contentBounds = titleBounds.BelowCopy(0.0, 10.0);
                ElementBounds containerBounds = new ElementBounds();
                containerBounds.BothSizing = ElementSizing.FitToChildren;
                containerBounds.WithFixedPadding(5.0, 5.0);
                ElementBounds screenBounds = ElementStdBounds.AutosizedMainDialog
                    .WithAlignment(EnumDialogArea.LeftTop)
                    .WithFixedAlignmentOffset(GuiStyle.DialogToScreenPadding, GuiStyle.DialogToScreenPadding);

                // LoadedTexture loadedTexture = null;
                // GuiElementRichtext richtext;

                if (composer == null)
                {
                    composer = capi.Gui.CreateCompo("seedanalysishud", screenBounds);
                }
                else
                {
                    // grabbing loadedTexture before recompose
                    // richtext = composer.GetRichtext("rt");
                    // loadedTexture = richtext.richtTextTexture;
                    // richtext.richtTextTexture = null;
                    composer.Clear(screenBounds);
                }

                ElementBounds statbarBounds = contentBounds.BelowCopy(0.0, 0.0, 0.0, 0.0);

                Composers["seedanalysishud"] = composer;
                composer
                    .AddGameOverlay(containerBounds)
                    .BeginChildElements(containerBounds)
                    .AddStaticTextAutoBoxSize(
                        title,
                        CairoFont.WhiteSmallishText(),
                        EnumTextOrientation.Center,
                        titleBounds,
                        "seedtitle"
                    )
                    // .AddRichtext(detail, CairoFont.WhiteDetailText(), contentBounds, "rt")
                    //*
                    .AddStaticTextAutoBoxSize(
                        detail,
                        CairoFont.WhiteDetailText(),
                        EnumTextOrientation.Center,
                        contentBounds,
                        "seeddetail"
                    )//*/
                    .AddStatbar(statbarBounds, new double[3] { 0.0, 1.0, 0.0 }, "seedstat1")
                    .EndChildElements();
                // richtext = composer.GetRichtext("rt");
                if (detail.Length == 0)
                {
                    contentBounds.fixedY = 0;
                    contentBounds.fixedHeight = 0;
                }

                // if (loadedTexture != null)
                // {
                // richtext.richtTextTexture = loadedTexture;
                // }

                // richtext.BeforeCalcBounds();
                // contentBounds.fixedWidth = Math.Min(500, richtext.MaxLineWidth / (double)RuntimeEnv.GUIScale + 1.0);

                var cTitle = composer.GetStaticText("seedtitle");
                var cDetails = composer.GetStaticText("seeddetail");
                var cStatbar = composer.GetStatbar("seedstat1");

                composer.Compose();
            }
        }

        public HudSeedAnalyzer WithBlockAndEntitySelection(BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel != null)
            {
                currentBlock = capi.World.BlockAccessor.GetBlock(blockSel.Position);
                if (currentBlock != null && currentBlock.BlockId == 0)
                {
                    currentBlock = null;
                }
            }

            /*
            else if (block != currentBlock || !currentPos.Equals(currentBlockSelection.Position) || currentSelectionIndex != currentBlockSelection.SelectionBoxIndex)
            {
                currentBlock = block;
                currentSelectionIndex = currentBlockSelection.SelectionBoxIndex;
                currentPos = (currentBlockSelection.DidOffset ? currentBlockSelection.Position.Copy().Add(currentBlockSelection.Face.Opposite) : currentBlockSelection.Position.Copy());
                ComposeSeedAnalysisHUD();
            }//*/

            if (entitySel != null)
            {
                currentEntity = entitySel.Entity;
            }

            ComposeSeedAnalysisHUD();

            return this;
        }

        /// <summary>
        /// Assumes seed items on ground are entities.
        /// </summary>
        /// <remarks>Gets all entities</remarks>
        private void SeedEntityScan()
        {
            Entity entity = capi.World.Player.CurrentEntitySelection.Entity;
            if (entity != null)
            {
                currentEntity = entity;
                ComposeSeedAnalysisHUD();
            }
        }

        /// <summary>
        /// Assumes Crop is Block. Also picks up Farmland.
        /// </summary>
        /// <remarks>Gets all blocks</remarks>
        private void CropScan()
        {
            BlockSelection currentBlockSelection = capi.World.Player.CurrentBlockSelection;
            Block block;
            if (currentBlockSelection.DidOffset)
            {
                BlockFacing opposite = currentBlockSelection.Face.Opposite;
                block = capi.World.BlockAccessor.GetBlock(currentBlockSelection.Position);
            }
            else
            {
                block = capi.World.BlockAccessor.GetBlock(currentBlockSelection.Position);
            }
            if (block.BlockId == 0)
            {
                currentBlock = null;
            }
            else if (block != currentBlock || !currentPos.Equals(currentBlockSelection.Position) || currentSelectionIndex != currentBlockSelection.SelectionBoxIndex)
            {
                currentBlock = block;
                currentSelectionIndex = currentBlockSelection.SelectionBoxIndex;
                currentPos = (currentBlockSelection.DidOffset ? currentBlockSelection.Position.Copy().Add(currentBlockSelection.Face.Opposite) : currentBlockSelection.Position.Copy());
                ComposeSeedAnalysisHUD();
            }
        }

    }

    /// <source href="https://github.com/anegostudios/vsmodexamples/blob/master/code_mods/HudOverlaySample/HudOverlaySample/WeirdProgressBarRenderer.cs" />
    internal class IdkWhatsGoingon : IRenderer
    {
        MeshRef whiteRectangleRef;
        MeshRef progressQuadRef;
        LoadedTexture texMessage;

        ICoreClientAPI capi;
        TextTextureUtil TextUtil;

        Matrixf mvMatrix = new Matrixf();


        public double RenderOrder { get { return 0; } }

        public int RenderRange { get { return 10; } }

        public IdkWhatsGoingon(ICoreClientAPI api)
        {
            capi = api;
            TextUtil = new TextTextureUtil(capi);

            OriginsLogger.Debug(api, "|-+ Created SeedAnalyzer");
            OriginsLogger.Debug(api, "| |");

            // This will get a line loop with vertices inside [-1,-1] till [1,1]
            MeshData rectangle = LineMeshUtil.GetRectangle(ColorUtil.WhiteArgb);
            whiteRectangleRef = api.Render.UploadMesh(rectangle);

            // This will get a quad with vertices inside [-1,-1] till [1,1]
            progressQuadRef = api.Render.UploadMesh(QuadMeshUtil.GetQuad());

            // This is a section for putting text into rectangle
            const string AnalysisPlaceholder = "Analysis:\na: 0\nb: 0\nc: 0";
            FontConfig fontCfg = new FontConfig
            {
                Fontname = GuiStyle.StandardFontName,
                StrokeColor = new double[4] { 0.5, 0.5, 0.5, 0.5 },
                UnscaledFontsize = 12.0,
            };
            CairoFont font = new CairoFont(fontCfg);

            texMessage = TextUtil.GenUnscaledTextTexture(AnalysisPlaceholder, font);
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            OriginsLogger.Debug(capi, "| |- Rendered SeedAnalyzer");

            IShaderProgram curShader = capi.Render.CurrentActiveShader;

            Vec4f color = new Vec4f(1, 1, 1, 1);

            // Render rectangle
            curShader.Uniform("rgbaIn", color);
            curShader.Uniform("extraGlow", 0);
            curShader.Uniform("applyColor", 0);
            curShader.Uniform("tex2d", 0);
            curShader.Uniform("noTexture", 1f);

            /*
            mvMatrix
                .Set(capi.Render.CurrentModelviewMatrix)
                .Translate(10, 10, 50)
                .Scale(100, 20, 0)
                .Translate(0.5f, 0.5f, 0)
                .Scale(0.5f, 0.5f, 0)
            ;//*/

            mvMatrix
                .Set(capi.Render.CurrentModelviewMatrix)
                .Translate(10, 10, 50)
                .Scale(texMessage.Width, texMessage.Height, 0)
                .Translate(0.5f, 0.5f, 0)
                .Scale(0.5f, 0.5f, 0.5f)
            ;

            curShader.UniformMatrix("projectionMatrix", capi.Render.CurrentProjectionMatrix);
            curShader.UniformMatrix("modelViewMatrix", mvMatrix.Values);

            capi.Render.RenderMesh(whiteRectangleRef);


            // Render progress bar
            /*
            float width = (capi.World.ElapsedMilliseconds / 10f) % 100;

            mvMatrix
                .Set(capi.Render.CurrentModelviewMatrix)
                .Translate(10, 10, 50)
                .Scale(width, 20, 0)
                .Translate(0.5f, 0.5f, 0)
                .Scale(0.5f, 0.5f, 0)
            ;
            
            //*/

            curShader.UniformMatrix("projectionMatrix", capi.Render.CurrentProjectionMatrix);
            curShader.UniformMatrix("modelViewMatrix", mvMatrix.Values);

            capi.Render.RenderMesh(progressQuadRef);

            RenderText(stage != EnumRenderStage.Opaque);
        }

        private void RenderText(bool isShadowPass)
        {
            IRenderAPI rpi = capi.Render;

            if (true)
            {
                rpi.CurrentActiveShader.Uniform("rgbaIn", new Vec4f(0f, 0f, 0f, 1f));
                rpi.CurrentActiveShader.Uniform("tex2d", 1);
                rpi.CurrentActiveShader.Uniform("noTexture", 0f);
                rpi.CurrentActiveShader.BindTexture2D("tex2d", texMessage.TextureId, 0);
            }
        }

        public void Dispose()
        {
            OriginsLogger.Debug(capi, "| \\ Disposing of SeedAnalyzer");
            texMessage.Dispose();
            capi.Render.DeleteMesh(whiteRectangleRef);
            capi.Render.DeleteMesh(progressQuadRef);
        }
    }
}
