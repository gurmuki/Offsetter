using Offsetter.Entities;
using Offsetter.Graphics;
using Offsetter.Io;
using Offsetter.Math;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Offsetter
{
    using GChainList = List<GChain>;

    using Keys = System.Windows.Forms.Keys;

    public partial class Offsetter : Form
    {
        // Initialized in ShadersCreate().
        private string vWireframeShaderCode = null!;
        private string fWireframeShaderCode = null!;
        private string vWindowingShaderCode = null!;
        private string fWindowingShaderCode = null!;

        private Shader wireframeShader = null!;
        private Shader windowingShader = null!;

        private WindowingRenderer windowingObject = null!;

        // center is used by GLxord() and GLyord() so they
        // can return coordinates in clip space [-1..+1]
        private PointD center;

        // For managing view changes.
        private ViewStack views = new ViewStack();

        private Matrix4 model;

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        enum MouseEvent { None, LButtonDown, LButtonDownMoving, LButtonUp }

        enum ViewMode { Static, Zooming, Windowing, Panning, Picking }
        private ViewMode viewMode { get; set; } = ViewMode.Static;

        private Point viewPt = new Point();
        private bool viewPtLocked = false;

        private GDxfReader dxfReader = new GDxfReader();
        private GChainList ichains = new GChainList();
        private GChainList ochains = new GChainList();

        private bool toolingEnabled = false;
        private GChainList tchains = new GChainList();

        private int offsetSide = GConst.LEFT;
        private double offsetDist = 1;

        // View management made simple: The application client
        // window is square, as are modelBox and viewBox.
        //
        // modelBox is created when a dxf upon reading a DXF
        // and represents the bounding box containing all entites.
        //
        // viewBox is the portion of the modelBox displayed
        // in the application client window. As implemented,
        // viewBox is updated exclusively by ViewUpdate().
        GBox modelBox = new GBox();
        GBox viewBox = new GBox();

        private Dictionary<GCurve, Renderer> rendererMap = new Dictionary<GCurve, Renderer>();
        private Dictionary<GCurve, GBox> boxMap = new Dictionary<GCurve, GBox>();

        private Dictionary<GChainType, VColor> colorMap = new Dictionary<GChainType, VColor>();

        // geoMenuLocation is used as a reference point for displaying dialogs.
        private Point geoMenuLocation;

        // selectionDialog is a modeless dialog whose lifetime must be managed.
        private SelectionDialog selectionDialog = null!;
        private List<GCurve> selectedCurves = new List<GCurve>();

        private const string PROPERTIES = "Properties";
        private const string UNIFORM = "Uniform Offset";
        private const string NON_UNIFORM = "Non-uniform Offset";
        private const string NESTING = "Nest";

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        public Offsetter()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(40, 20);

            // CRITICAL: KeyPreview must be set 'true' to intercept
            // key presses for view changing.
            KeyPreview = true;

            // Make sure that when the GLControl is resized or needs to be painted,
            // we update our projection matrix or re-render its contents, respectively.
            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;

            // Ensure that the viewport and projection matrix are set correctly initially.
            glControl_Resize(glControl, EventArgs.Empty);

            // Hookup the key and mouse event handlers.
            GLControlEventHandlersBind();

            GLSetup();

            FormResize();

            MenusItemsEnable(false);

            geoMenuLocation = this.PointToScreen(new Point(50, 25));
        }

        private void Offsetter_FormClosing(object sender, FormClosingEventArgs e)
        {
            // NOTE: Whatever the reason, after implementing Properties as a modeless
            // dialog, the main window would no longer close when pressing the X button.
            // It's hardly worth the bother to understand why.
            e.Cancel = false;
        }

        private void Offsetter_FormClosed(object sender, FormClosedEventArgs e)
        {
            RendererMapClear();

            if (windowingObject != null)
                windowingObject.Dispose();

            ichains.Clear();
            ochains.Clear();
            tchains.Clear();
        }

        private void GLSetup()
        {
            ShadersCreate();

            windowingObject = new WindowingRenderer(windowingShader);

            model = Matrix4.Identity;

            colorMap[GChainType.PART] = VColor.GREEN;
            colorMap[GChainType.POCKET] = VColor.RED;
            colorMap[GChainType.ISLAND] = VColor.RED;
            colorMap[GChainType.PATH] = VColor.RED;
            colorMap[GChainType.TOOL] = VColor.BLUE;
        }

        private void ShadersCreate()
        {
            // The shader code could (of course) be loaded from files.
            // https://opentk.net/learn/chapter1/8-coordinate-systems.html
            #region ShaderCodeRegion
            // Vertex shading code.
            vWireframeShaderCode =
                @"#version 330 core

                layout(location = 0) in vec2 vrtx2d;

                vec3 vrtx3d;

                uniform mat4 model;

                void main(void)
                {
                    vrtx3d = vec3(vrtx2d, 0);

                    gl_Position = vec4(vrtx3d, 1.0) * model;
                }";

            // Fragment shading code.
            fWireframeShaderCode =
                @"#version 330 core

                out vec4 outputColor;

                uniform vec4 curveColor;

                void main()
                {
                    outputColor = curveColor;
                }";

            // Vertex shading code.
            vWindowingShaderCode =
                @"#version 330 core

                layout(location = 0) in vec2 vrtx2d;

                vec3 vrtx3d;

                void main(void)
                {
                    vrtx3d = vec3(vrtx2d, 0.1);

                    gl_Position = vec4(vrtx3d, 1.0);
                }";

            // Fragment shading code.
            fWindowingShaderCode =
                @"#version 330 core

                out vec4 outputColor;

                uniform vec4 curveColor;

                void main()
                {
                    outputColor = curveColor;
                }";
            #endregion

            wireframeShader = Shader.FromCode(vWireframeShaderCode, fWireframeShaderCode);
            windowingShader = Shader.FromCode(vWindowingShaderCode, fWindowingShaderCode);
        }

        private void glControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            KeyPreviewExecute(e.KeyCode);
        }

        private void glControl_Resize(object? sender, EventArgs e)
        {
            glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);

            center.X = (glControl.Width / 2);
            center.Y = (glControl.Height / 2);
        }

        private void glControl_Paint(object? sender, PaintEventArgs e) => Render();

        private void FormResize()
        {
            int dimension = menuStrip1.Width;

            mainPanel.Location = new Point(MainMenuStrip!.Left, MainMenuStrip.Bottom);
            mainPanel.Size = new Size(dimension, this.Height - MainMenuStrip.Bottom);

            glControl.Location = new Point(0, 0);
            glControl.Size = mainPanel.Size;

            this.Invalidate();
        }

        private void fileMenu_DropDownOpening(object sender, EventArgs e)
        {
            reopenFileMenuItem.Text = "Reopen";
            reopenFileMenuItem.Enabled = false;

            string path = Properties.Settings.Default.inputPath;
            if (!String.IsNullOrEmpty(path) && File.Exists(path))
            {
                reopenFileMenuItem.Text += String.Format(" ({0})", Path.GetFileName(path));
                reopenFileMenuItem.Enabled = true;
            }
        }

        // File Menu actions.
        private void openFileMenuItem_Click(object sender, EventArgs e) => DxfOpen(null!);
        private void reopenFileMenuItem_Click(object sender, EventArgs e) => DxfOpen(Properties.Settings.Default.inputPath);
        private void saveAsFileMenuItem_Click(object sender, EventArgs e) => DxfSave(null!);
        private void saveResultsFileMenuItem_Click(object sender, EventArgs e) => ResultSave();
        private void testFileMenuItem_Click(object sender, EventArgs e) => Test();

        // Geometry Menu actions.
        private void uniformOffsetGeometryMenuItem_Click(object sender, EventArgs e) => SelectionDialogShow(UNIFORM);
        private void nonUniformOffsetGeometryMenuItem_Click(object sender, EventArgs e) => SelectionDialogShow(NON_UNIFORM);
        private void nestGeometryMenuItem_Click(object sender, EventArgs e) => SelectionDialogShow(NESTING);
        private void decomposeGeometryMenuItem_Click(object sender, EventArgs e) => Decompose();
        private void toolingGeometryMenuItem_Click(object sender, EventArgs e) => Tooling();
        private void reorderGeometryMenuItem_Click(object sender, EventArgs e) => Reorder();
        private void propertiesGeometryMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.P);

        // View Menu actions.
        private void panViewMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.C);
        private void windowViewMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.W);
        private void zoomViewMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.Z);
        private void fullViewMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.F);
        private void previousViewMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.V);

        // Context Menu actions.
        private void panContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.C);
        private void windowContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.W);
        private void zoomContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.Z);
        private void fullViewContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.F);
        private void previousViewContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.V);
        private void propertiesContextMenuItem_Click(object sender, EventArgs e) => PreviewKeyEvent(Keys.P);

        private void PreviewKeyEvent(Keys keyCode) => KeyPreviewExecute(keyCode);

        private void MenusEnable(bool enable)
        {
            menuStrip1.Enabled = enable;
            viewPopMenu.Enabled = enable;
        }

        private void MenusItemsEnable(bool enable)
        {
            bool haveInputGeometry = (ichains.Count > 0);

            // Geometry Menu Items.
            uniformOffsetGeometryMenuItem.Enabled = (enable && haveInputGeometry);
            nonUniformOffsetGeometryMenuItem.Enabled = (enable && (ichains.Count == 2));
            nestGeometryMenuItem.Enabled = (enable && (ichains.Count == 2));
            decomposeGeometryMenuItem.Enabled = (enable && (ichains.Count == 1));
            reorderGeometryMenuItem.Enabled = enable;
            propertiesGeometryMenuItem.Enabled = (enable && (ichains.Count > 0));

            // View Menu Items.
            panViewMenuItem.Enabled = (enable && haveInputGeometry);
            windowViewMenuItem.Enabled = (enable && haveInputGeometry);
            zoomViewMenuItem.Enabled = (enable && haveInputGeometry);
            fullViewMenuItem.Enabled = (enable && haveInputGeometry);
            previousViewMenuItem.Enabled = (enable && haveInputGeometry);

            // Context Menu Items.
            panContextMenuItem.Enabled = (enable && haveInputGeometry);
            windowContextMenuItem.Enabled = (enable && haveInputGeometry);
            zoomContextMenuItem.Enabled = (enable && haveInputGeometry);
            fullViewContextMenuItem.Enabled = (enable && haveInputGeometry);
            previousViewContextMenuItem.Enabled = (enable && haveInputGeometry);

            saveAsContextMenuItem.Enabled = enable;
            saveResultsFileMenuItem.Enabled = enable;

            if (!enable)
                ToolingMenuItemEnable(false);
        }

        private void ToolingMenuItemEnable(bool enable)
        {
            toolingGeometryMenuItem.Enabled = enable;
            if (!enable)
                toolingEnabled = false;
        }

        private void Render()
        {
            glControl.MakeCurrent();

            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            int previousProgramID = -1;
            foreach (var pair in rendererMap)
            {
                Renderer renderer = pair.Value;
                if (renderer.programID != previousProgramID)
                {
                    renderer.SetMatrix4("model", model);
                    previousProgramID = renderer.programID;
                }

                renderer.Render();
            }

            if (windowingObject.IsEnabled)
                windowingObject.Render();

            glControl.SwapBuffers();
        }

        private void RendererMapClear()
        {
            foreach (var renderer in rendererMap.Values)
            { renderer.Dispose(); }

            rendererMap.Clear();
        }
    }
}
