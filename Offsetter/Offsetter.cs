﻿using Offsetter.Entities;
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

        // Part and Tool are used by ConvexHullOffset() and Nest().
        // These exist to "simplify" related code.
        private GChain Part { get { return ichains[0]; } }
        private GChain Tool { get { return ichains[1]; } }

        private Dictionary<GCurve, Renderer> rendererMap = new Dictionary<GCurve, Renderer>();
        private Dictionary<GCurve, GBox> boxMap = new Dictionary<GCurve, GBox>();

        private Dictionary<GChainType, VColor> colorMap = new Dictionary<GChainType, VColor>();

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

            MenusEnable(false);
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

        private void glControl_Paint(object? sender, PaintEventArgs e)
        {
            Render();
        }

        private void FormResize()
        {
            int dx = this.ClientSize.Width;
            int dy = this.ClientSize.Height - MainMenuStrip!.Height;
            int dimension = ((dx < dy) ? dy : dx);

            this.ClientSize = new Size(dimension, dimension + MainMenuStrip!.Height);

            glControl.Location = new System.Drawing.Point(0, MainMenuStrip!.Bottom);
            glControl.Size = new Size(dimension, dimension);

            this.Invalidate();
        }

        private void PreviewKeyEvent(Keys keyCode)
        {
            KeyPreviewExecute(keyCode);
        }

        private void panToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviewKeyEvent(Keys.C);
        }

        private void windowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviewKeyEvent(Keys.W);
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviewKeyEvent(Keys.Z);
        }

        private void fullViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviewKeyEvent(Keys.F);
        }

        private void previousViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PreviewKeyEvent(Keys.U);
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            reopenToolStripMenuItem.Text = "Reopen";
            reopenToolStripMenuItem.Enabled = false;

            string path = Properties.Settings.Default.inputPath;
            if (!String.IsNullOrEmpty(path) && File.Exists(path))
            {
                reopenToolStripMenuItem.Text += String.Format(" ({0})", Path.GetFileName(path));
                reopenToolStripMenuItem.Enabled = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DxfOpen(null!);
        }

        private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DxfOpen(Properties.Settings.Default.inputPath);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DxfSave(null!);
        }

        private void saveresultsdxfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResultSave();
        }

        private void uniformOffsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UniformOffset();
        }

        private void convexHullOffsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvexHullOffset();
        }

        private void nestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Nest();
        }

        private void decomposeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Decompose();
        }

        private void toolingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tooling();
        }

        private void reorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reorder();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Test();
        }

        private void MenusEnable(bool enable)
        {
            uniformOffsetToolStripMenuItem.Enabled = (enable && (ichains.Count > 0));
            convexHullOffsetToolStripMenuItem.Enabled = (enable && (ichains.Count == 2));
            nestToolStripMenuItem.Enabled = (enable && (ichains.Count == 2));
            decomposeToolStripMenuItem.Enabled = (enable && (ichains.Count == 1));

            reorderToolStripMenuItem.Enabled = enable;
            saveAsToolStripMenuItem.Enabled = enable;
            saveresultsdxfToolStripMenuItem.Enabled = enable;

            if (!enable)
                ToolingMenuItemEnable(false);
        }

        private void ToolingMenuItemEnable(bool enable)
        {
            toolingToolStripMenuItem.Enabled = enable;
            if (!enable)
                toolingEnabled = false;
        }

        private void Render()
        {
            glControl.MakeCurrent();

            GL.ClearColor(Color.WhiteSmoke);
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
