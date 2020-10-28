using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.Linq;

using ImGuiSetup;
using ImGuiNET;

namespace PolyditchGameEngine
{
    public class RenderPipeline
    {
        public virtual void OnLoad()
        {

        }
        public virtual void OnRender()
        {

        }
    }
    enum PolyditchRenderPipeline
    {
        LightWeightRenderPipeline,
        SoftwareRenderPipeline
    }
    public class Window : GameWindow
    {
        public ImGuiController Controller;

        public Window(string Title, int Width, int Height) : base(Width, Height, GraphicsMode.Default, Title)
        {

        }
    }

    public class PolyDitchGameEngine : Window
    {
        // Basic Items
        public static Shader BasicShader;
        public static Texture BasicTexture;
        public static Material BasicMaterial;
        // Basic Items End
        public static List<Shader> Shaders = new List<Shader>(0);
        public static List<Camera> Cameras = new List<Camera>(0);
        public static List<GameObject> GameObjects = new List<GameObject>(0);
        public static List<MeshRenderer> MeshesToRender = new List<MeshRenderer>(0);
        public static void AddGameObject(GameObject gameObject)
        {
            GameObjects.Add(gameObject);
        }
        public static void DeleteGameObject(GameObject gameObject)
        {
            GameObjects.Remove(gameObject);
        }
        public static int Width;
        public static int Height;
        public static Mesh BasicMesh;
        public static List<Mesh> Meshes = new List<Mesh>(0);
        public static float speed = 1;
        public static float DeltaTime;
        NativeWindow NativeWindow = new NativeWindow(800, 800, "Test", GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);
        public static Camera MainCamera = new Camera(new Vector3(0, 0, -3.0f), new Vector3(179, 0, 90), new Vector3(0.19f,0.61f,0.65f));
        public static void AddMesh(Mesh mesh)
        {
            Meshes.Add(mesh);
        }
        public PolyDitchGameEngine(int width, int height, string Title) : base(Title, width, height)
        {
            BasicMesh = new Mesh(@"Assets\dragon.obj");
            BeforeWindowLoads();
            BasicShader = new Shader("Vertex.glsl", "Fragment.glsl");
            BasicTexture = new Texture(@"Assets\Basic_Texture.png");
            BasicMaterial = new Material(BasicTexture, BasicShader, 1, 10);
            Width = base.Width;
            Height = base.Height;
            base.Run(60);
        }
        protected override void OnLoad(EventArgs e)
        {
            OnStart();

            Controller = new ImGuiController(Width, Height);

            GL.ClearColor(0.19f, 0.61f, 0.65f, 1.0f);
            //GL.MatrixMode(MatrixMode.Projection);
            GraphicsEngine.LightWeight3DRenderPipeline.OnLoad();
            foreach(GameObject gameObject in GameObjects)
            {
                if (gameObject.Enabled)
                {
                    foreach(PolyDitch polyDitch in gameObject.Scripts)
                    {
                        polyDitch.OnStart();
                    }
                }
            }
            base.OnLoad(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            OnRender();
            //GraphicsEngine.LightWeight3DRenderPipeline.OnRender(this);
            GraphicsEngine.SoftwareRenderPipeline.OnRender(Controller, this);
            base.OnRenderFrame(e);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Input.Update();
            Time.Update(e.Time);
            foreach (GameObject gameObject in GameObjects)
            {
                if (gameObject.Enabled)
                {
                    foreach (PolyDitch polyDitch in gameObject.Scripts)
                    {
                        polyDitch.OnUpdate();
                    }
                }
            }

            OnUpdate();
            base.OnUpdateFrame(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = base.Width;
            Height = base.Height;
            GL.Viewport(0, 0, base.Width, base.Height);
            Controller.WindowResized(base.Width, base.Height);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            Controller.PressChar(e.KeyChar);

        }
        protected override void OnUnload(EventArgs e)
        {
            GraphicsEngine.LightWeight3DRenderPipeline.OnUnload();
            base.OnUnload(e);
        }
        public virtual void BeforeWindowLoads()
        {

        }
        public virtual void OnRender()
        {

        }
        public virtual void OnUpdate()
        {

        }
        public virtual void OnStart()
        {

        }
        private static class GraphicsEngine
        {
            
            //private static Matrix4 Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), Width / Height, 0.1f, 100f);
            private static Matrix4 model = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90));
            private static Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            private static Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)Width / (float)Height, 0.1f, 100.0f);
            private static List<VAO> VAOs = new List<VAO>(0);
            public static class LightWeight3DRenderPipeline
            {
                private static float[] vertices = {
                  //Position          Texture coordinates
                    .5f,  0.5f, 0.0f,
                    0.5f, -0.5f, 0.0f,  // bottom right
                    -0.5f, -0.5f, 0.0f,  // bottom left
                    -0.5f,  0.5f, 0.0f  // top left
                };
                private static float[] TextureCordinates =
                    {
                    1.0f, 1.0f, // top right
                    1.0f, 0.0f, // bottom right
                    0.0f, 0.0f, // bottom left
                    0.0f, 1.0f  // top left
                    };
                private static uint[] indices = {  // note that we start from 0!
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
                };
                private static float[] Colors =
                {
                0,1,0,1,
                1,1,0,1,
                1,1,0,1,
                1,0,0,1
                 };
                private static GlobalLight BasicLight = new GlobalLight(new Vector3(0, 0, -20), new Vector3(1, 1, 1));
                private static List<VAO> VAOs = new List<VAO>(0);

                private static float distance = -3;

                public static void OnLoad()
                {
                    //Console.WriteLine(Height);
                    for (int x = 0; x < PolyDitchGameEngine.MeshesToRender.Count; x++)
                    {
                        VAOs.Add(PolyDitchGameEngine.MeshesToRender[x].Mesh.vao);
                    }
                    foreach(Shader shader in Shaders)
                    {
                        ShaderSetUp(shader);
                    }
                    //vao = new VAO(indices, new VertexArray(3, vertices), new VertexArray(4, Colors), new VertexArray(2, TextureCordinates));
                }
                public static void FrameSetup()
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                }
                public static void FrameCleanup(GameWindow Window)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                    GL.UseProgram(0);

                    Window.SwapBuffers();
                }
                private static void ShaderSetUp(Shader CurrentShader)
                {
                    CurrentShader.Use();
                    CurrentShader.UniformInt("texture0", 0);
                    CurrentShader.UniformInt("texture1", 1);
                    CurrentShader.UniformInt("texture2", 2);
                    CurrentShader.UniformInt("texture3", 3);
                    CurrentShader.UniformInt("texture4", 4);
                }
                public static void OnUnload()
                {
                    foreach(VAO vao in VAOs)
                    {
                        vao.Delete();
                    }
                    BasicShader.DeleteShader();
                }
                private static void TerrainRender(VAO vao)
                {
                    TerrainTexturePack TexturePack = vao.Terrain.TerrainTexturePack;
                    TexturePack.Texture0.UseTexture(TextureUnit.Texture0);

                    TexturePack.RTexture.UseTexture(TextureUnit.Texture1);

                    TexturePack.GTexture.UseTexture(TextureUnit.Texture2);

                    TexturePack.BTexture.UseTexture(TextureUnit.Texture3);

                    TexturePack.BlendMap.UseTexture(TextureUnit.Texture4);
                }
                private static void CreateProjection()
                {
                    float aspectRatio = (float)Width / Height;
                    projection = Matrix4.CreatePerspectiveFieldOfView(
                        60 * ((float)Math.PI / 180f), // field of view angle, in radians
                        aspectRatio,                // current window aspect ratio
                        0.1f,                       // near plane
                        4000f);                     // far plane
                }
                public static void OnRender(GameWindow Window)
                {
                    FrameSetup();

                    //BasicLight.Position = MainCamera.Position;
                    foreach(Camera camera in Cameras)
                    {
                        for (int i = 0; i < VAOs.Count; i++)
                        {
                            VAOs[i].MeshRenderer.Material.Shader.Use();
                            CreateProjection();
                            GL.Enable(EnableCap.CullFace);
                            GL.CullFace(CullFaceMode.Back);
                            if (VAOs[i].SpecialInformation == "Terrain")
                            {
                                TerrainRender(VAOs[i]);
                            }
                            else
                            {
                                VAOs[i].MeshRenderer.Material.Texture.UseTexture(TextureUnit.Texture0);
                            }
                            GL.BindVertexArray(VAOs[i].vao);
                            VAOs[i].MeshRenderer.Material.Shader.UniformMatrix4("model", PolyDitchGameEngine.MeshesToRender[i].ModelTransformation);
                            VAOs[i].MeshRenderer.Material.Shader.UniformMatrix4("view", camera.view);
                            VAOs[i].MeshRenderer.Material.Shader.UniformMatrix4("projection", projection);
                            VAOs[i].MeshRenderer.Material.Shader.UniformVector("LightPosition", BasicLight.Position.X, BasicLight.Position.Y, BasicLight.Position.Z, 0);
                            VAOs[i].MeshRenderer.Material.Shader.UniformVector("LightColor", BasicLight.Color.X, BasicLight.Color.Y, BasicLight.Color.Z);
                            VAOs[i].MeshRenderer.Material.Shader.UniformFloat("Reflectivity", PolyDitchGameEngine.MeshesToRender[i].Material.Reflectivity);
                            VAOs[i].MeshRenderer.Material.Shader.UniformFloat("ShineDampening", PolyDitchGameEngine.MeshesToRender[i].Material.ShineDampening);
                            VAOs[i].MeshRenderer.Material.Shader.UniformVector("SkyColor", camera.SkyColor.X, camera.SkyColor.Y, camera.SkyColor.Z, 1);
                            GL.DrawElements(PrimitiveType.Triangles, VAOs[i].NumberOfIndicies, DrawElementsType.UnsignedInt, 0);
                            GL.BindVertexArray(0);
                            //FrameCleanup();
                        }
                    }
                    /*
                    GL.BindVertexArray(vao.vao);
                    BasicTexture.UseTexture();
                    BasicShader.UniformMatrix4("model", model);
                    BasicShader.UniformMatrix4("view", view);
                    //BasicShader.UniformMatrix4("projection", projection);
                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
                    */
                    FrameCleanup(Window);
                }
            }

            public static class SoftwareRenderPipeline
            {
                public static void OnRender(ImGuiController Controller, GameWindow Window)
                {
                    Controller.Update(Window, Time.DeltaTime);

                    GL.ClearColor(new Color4(0, 32, 48, 255));
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                    ImGui.ShowDemoWindow();

                    Controller.Render();

                    Util.CheckGLError("End of frame");

                    Window.SwapBuffers();

                }
            }

        }
    }
    public class VertexArray
    {
        public float[] Data;
        public int Size;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">This is the dimenional size of each vertex in Data, must be 1,2,3, or 4.</param>
        /// <param name="Data">This is the array of verticies.</param>
        public VertexArray(int size, float[] Data)
        {
            this.Data = Data;
            this.Size = size;
        }
        public float this[int index]
        {
            get
            {
                return this.Data[index];
            }
            set
            {
                Data[index] = value;
            }
        }

        public int Length => Data.Length;
    }
    public class VAO
    {
        public int vao;
        public List<int> vbo = new List<int>(0);
        public int veo;
        public string SpecialInformation = "";
        public MeshRenderer MeshRenderer;
        public Terrain Terrain;
        public int NumberOfIndicies;
        public VAO(uint[] Indicies, params VertexArray[] Data)
        {
            NumberOfIndicies = Indicies.Length;
            this.vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            for (int x = 0; x < Data.Length; x++)
            {
                VertexArray Temp = Data[x];
                vbo.Add(GL.GenBuffer());
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo[x]);
                GL.BufferData(BufferTarget.ArrayBuffer, Temp.Length * sizeof(float), Temp.Data, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(x, Temp.Size, VertexAttribPointerType.Float, false, 0, 0);
                GL.EnableVertexAttribArray(x);
            }
            veo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, veo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indicies.Length * sizeof(uint), Indicies, BufferUsageHint.StaticDraw);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        public void Delete()
        {
            foreach (int vbo in vbo)
            {
                GL.DeleteBuffer(vbo);
            }
            GL.DeleteBuffer(veo);
            GL.DeleteVertexArray(vao);
        }

    }
    public class Material
    {
        public Texture Texture;
        public TerrainTexturePack TerrainTexturePack;
        public Shader Shader;
        public float Reflectivity;
        public float ShineDampening;
        public Material(Texture Texture, Shader Shader, float Reflectivity, float ShineDampening)
        {
            this.Texture = Texture;
            this.Shader = Shader;
            this.Reflectivity = Reflectivity;
            this.ShineDampening = ShineDampening;
        }
        public Material(TerrainTexturePack TerrainTexturePack, Shader Shader, float Reflectivity, float ShineDampening)
        {
            this.TerrainTexturePack = TerrainTexturePack;
            this.Shader = Shader;
            this.Reflectivity = Reflectivity;
            this.ShineDampening = ShineDampening;
        }
    }
    public class Mesh
    {
        public VAO vao;
        private enum FileType
        {
            Null,
            obj
        }
        public float[] Verticies;
        public float[] TextureCoordinates;
        public float[] Normals;
        public uint[] indicies;
        private void LoadFromOBJ(string FilePath)
        {
            List<float> verts = new List<float>(0);
            List<float> TextC = new List<float>(0);
            List<float> norms = new List<float>(0);

            List<float> v = new List<float>(0);
            List<float> t = new List<float>(0);
            List<float> n = new List<float>(0);
            List<uint> i = new List<uint>(0);

            float[] V = new float[0];
            float[] T = new float[0];
            float[] N = new float[0];

            string line = "";
            StreamReader file = new StreamReader(FilePath);
            while(line != null)
            {
                line = file.ReadLine();
                if (line.StartsWith("v "))
                {
                    string[] Vertex = line.Split(' ');
                    string V1 = Vertex[1];
                    string V2 = Vertex[2];
                    string V3 = Vertex[3];
                    verts.Add(float.Parse(V1));
                    verts.Add(float.Parse(V2));
                    verts.Add(float.Parse(V3));
                }
                if (line.StartsWith("vt "))
                {
                    string[] Vertex = line.Split(' ');
                    string V1 = Vertex[1];
                    string V2 = Vertex[2];
                    TextC.Add(float.Parse(V1));
                    TextC.Add(float.Parse(V2));
                }
                if (line.StartsWith("vn "))
                {
                    string[] Vertex = line.Split(' ');
                    string V1 = Vertex[1];
                    string V2 = Vertex[2];
                    string V3 = Vertex[3];
                    norms.Add(float.Parse(V1));
                    norms.Add(float.Parse(V2));
                    norms.Add(float.Parse(V3));
                }
                if(line.StartsWith("f "))
                {
                    T = new float[(verts.Count / 3) * 2];
                    N = new float[verts.Count];
                    break;
                }
            }
            uint counter = 0;
            while (line != null)
            {
                if (line.StartsWith("f "))
                {
                    #region Faces
                    string[] currentLine = line.Split(' ');
                    string[] v1 = currentLine[1].Split('/');
                    string[] v2 = currentLine[2].Split('/');
                    string[] v3 = currentLine[3].Split('/');
                    #endregion
                    #region Indicies
                    //i.Add(counter);
                    //i.Add(counter + 1);
                    //i.Add(counter + 2);
                    uint CurrentVertexPointer = uint.Parse(v1[0]) - 1;
                    uint CurrentVertexPointer1 = uint.Parse(v2[0]) - 1;
                    uint CurrentVertexPointer2 = uint.Parse(v3[0]) - 1;
                    i.Add(CurrentVertexPointer);
                    i.Add(CurrentVertexPointer1);
                    i.Add(CurrentVertexPointer2);
                    #endregion
                    #region Verticies
                    //v.Add(verts[(int.Parse(v1[0]) - 1) * 3]);
                    //v.Add(verts[(int.Parse(v1[0]) - 1) * 3] + 1);
                    //v.Add(verts[(int.Parse(v1[0]) - 1) * 3] + 2);

                    //v.Add(verts[(int.Parse(v2[0]) - 1) * 3]);
                    //v.Add(verts[(int.Parse(v2[0]) - 1) * 3] + 1);
                    //v.Add(verts[(int.Parse(v2[0]) - 1) * 3] + 2);

                    //v.Add(verts[(int.Parse(v3[0]) - 1) * 3]);
                    //v.Add(verts[(int.Parse(v3[0]) - 1) * 3] + 1);
                    //v.Add(verts[(int.Parse(v3[0]) - 1) * 3] + 2);
                    #endregion
                    #region Normals
                    N[(int)CurrentVertexPointer * 3 + 0] = norms[(int.Parse(v1[2]) - 1) * 3 + 0];
                    N[(int)CurrentVertexPointer * 3 + 1] = norms[(int.Parse(v1[2]) - 1) * 3 + 1];
                    N[(int)CurrentVertexPointer * 3 + 2] = norms[(int.Parse(v1[2]) - 1) * 3 + 2];

                    N[(int)CurrentVertexPointer1 * 3 + 0] = norms[(int.Parse(v2[2]) - 1) * 3 + 0];
                    N[(int)CurrentVertexPointer1 * 3 + 1] = norms[(int.Parse(v2[2]) - 1) * 3 + 1];
                    N[(int)CurrentVertexPointer1 * 3 + 2] = norms[(int.Parse(v2[2]) - 1) * 3 + 2];

                    N[(int)CurrentVertexPointer2 * 3 + 0] = norms[(int.Parse(v3[2]) - 1) * 3 + 0];
                    N[(int)CurrentVertexPointer2 * 3 + 1] = norms[(int.Parse(v3[2]) - 1) * 3 + 1];
                    N[(int)CurrentVertexPointer2 * 3 + 2] = norms[(int.Parse(v3[2]) - 1) * 3 + 2];
                    #endregion
                    #region Texture Coordinates
                    T[(int)CurrentVertexPointer * 2 + 0] = TextC[(int.Parse(v1[1]) - 1) * 2 + 0];
                    T[(int)CurrentVertexPointer * 2 + 1] = 1 - TextC[(int.Parse(v1[1]) - 1) * 2 + 1];

                    T[(int)CurrentVertexPointer1 * 2 + 0] = TextC[(int.Parse(v2[1]) - 1) * 2 + 0];
                    T[(int)CurrentVertexPointer1 * 2 + 1] = 1 - TextC[(int.Parse(v2[1]) - 1) * 2 + 1];

                    T[(int)CurrentVertexPointer2 * 2 + 0] = TextC[(int.Parse(v3[1]) - 1) * 2 + 0];
                    T[(int)CurrentVertexPointer2 * 2 + 1] = 1 - TextC[(int.Parse(v3[1]) - 1) * 2 + 1];
                    #endregion

                    counter += 3;
                }
                line = file.ReadLine();
            }
            //this.Verticies = v.ToArray();
            this.Verticies = verts.ToArray();
            this.Normals = N;
            this.TextureCoordinates = T;
            this.indicies = i.ToArray();
        }
        public Mesh(string FilePath)
        {
            FileType fileType = FileType.Null;
            if (FilePath.Length > 4)
            {
                if (FilePath.Substring(FilePath.Length - 4) == ".obj")
                {
                    fileType = FileType.obj;
                }
            }
            else
            {
                Console.WriteLine("The file path provided for the 3D file is invalid.");
                return;
            }
            if (fileType == FileType.Null)
            {
                Console.WriteLine("The file provided is not a recognised 3D File Type.");
                return;
            }
            if (fileType == FileType.obj)
            {
                LoadFromOBJ(FilePath);
                PolyDitchGameEngine.AddMesh(this);
                Console.WriteLine("Obj file from, " + FilePath + " has loaded");
            }
            this.vao = new VAO(this.indicies, new VertexArray(3, this.Verticies), new VertexArray(2, this.TextureCoordinates), new VertexArray(3, this.Normals));
        }
        public Mesh(float[] Verticies, float[] TextureCoordinates, float[] Normals, uint[] indicies)
        {
            this.Verticies = Verticies;
            this.TextureCoordinates = TextureCoordinates;
            this.Normals = Normals;
            this.indicies = indicies;
            this.vao = new VAO(this.indicies, new VertexArray(3, this.Verticies), new VertexArray(2, this.TextureCoordinates), new VertexArray(3, this.Normals));
        }
    }
    public class Shader
    {
        public readonly int Program;
        private int VertexShader;
        private int FragmentShader;
        public Shader(string VertexShaderFilePath, string FragmentShaderFilePath)
        {
            string VertexShaderCode = File.ReadAllText(VertexShaderFilePath);
            string FragmentShaderCode = File.ReadAllText(FragmentShaderFilePath);

            this.VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(this.VertexShader, VertexShaderCode);
            GL.CompileShader(this.VertexShader);
            string GLSLCompilerCheck = GL.GetShaderInfoLog(this.VertexShader);
            if (GLSLCompilerCheck != String.Empty)
            {
                Console.WriteLine(GLSLCompilerCheck);
            }

            this.FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(this.FragmentShader, FragmentShaderCode);
            GL.CompileShader(this.FragmentShader);
            GLSLCompilerCheck = GL.GetShaderInfoLog(this.FragmentShader);
            if (GLSLCompilerCheck != String.Empty)
            {
                Console.WriteLine(GLSLCompilerCheck);
            }

            Program = GL.CreateProgram();

            GL.AttachShader(Program, this.VertexShader);
            GL.AttachShader(Program, this.FragmentShader);

            GL.LinkProgram(Program);

            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out var code);

            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                Console.WriteLine("Linking error");
                Console.WriteLine(GL.GetProgramInfoLog(Program));
            }
            PolyDitchGameEngine.Shaders.Add(this);
        }
        public void Use()
        {
            GL.UseProgram(Program);
        }
        public void UniformFloat(string UniformName, float Float)
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.Uniform1(location, Float);
        }
        public void UniformInt(string UniformName, int Int)
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.Uniform1(location, Int);
        }
        public void UniformVector(string UniformName, float x, float y) 
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.Uniform2(location, x, y);
        }
        public void UniformVector(string UniformName, float x, float y, float z)
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.Uniform3(location, x, y, z);
        }
        public void UniformVector(string UniformName, float x, float y, float z, float w)
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.Uniform4(location, x, y, z, w);
        }
        public void UniformMatrix4(string UniformName, Matrix4 Data)
        {
            int location = GL.GetUniformLocation(Program, UniformName);
            GL.UniformMatrix4(location, false, ref Data);
            //GL.ProgramUniformMatrix4(Program, location, false, ref Data);
        }
        public void DeleteShader()
        {
            GL.DetachShader(Program, this.VertexShader);
            GL.DetachShader(Program, this.FragmentShader);
            GL.DeleteProgram(Program);
            GL.DeleteShader(this.VertexShader);
            GL.DeleteShader(this.FragmentShader);
        }
    }
    public class vector
    {
        
    }
    public class vector2 : vector
    {
        public float x = 0;
        public float y = 0;
        public vector2()
        {

        }
        public vector2(Vector2 Vector2)
        {
            this.x = Vector2.X;
            this.y = Vector2.Y;
        }
        public vector2(float X, float Y)
        {
            this.x = X;
            this.y = Y;
        }
        public static explicit operator Vector2(vector2 v2)
        {
            return new Vector2(v2.x, v2.y);
        }
        public static explicit operator vector3(vector2 v2)
        {
            return new vector3(v2.x, v2.y, 0);
        }
        public static explicit operator vector4(vector2 v2)
        {
            return new vector4(v2.x, v2.y, 0, 0);
        }
        public static vector2 operator +(vector2 vector0, vector2 vector1)
        {
            return new vector2(vector0.x + vector1.x, vector0.y + vector1.y);
        }
        public static vector2 operator *(vector2 vector0, vector2 vector1)
        {
            return new vector2(vector0.x * vector1.x, vector0.y * vector1.y);
        }
        public static vector2 operator -(vector2 vector0, vector2 vector1)
        {
            return new vector2(vector0.x - vector1.x, vector0.y - vector1.y);
        }
        public static vector2 operator /(vector2 vector0, vector2 vector1)
        {
            return new vector2(vector0.x / vector1.x, vector0.y / vector1.y);
        }
        public static vector2 operator +(vector2 vector0, float float1)
        {
            return new vector2(vector0.x + float1, vector0.y + float1);
        }
        public static vector2 operator *(vector2 vector0, float float1)
        {
            return new vector2(vector0.x * float1, vector0.y * float1);
        }
        public static vector2 operator -(vector2 vector0, float float1)
        {
            return new vector2(vector0.x - float1, vector0.y - float1);
        }
        public static vector2 operator /(vector2 vector0, float float1)
        {
            return new vector2(vector0.x / float1, vector0.y / float1);
        }

    }
    public class vector3 : vector
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public vector3()
        {

        }
        public vector3(Vector3 Vector3)
        {
            this.x = Vector3.X;
            this.y = Vector3.Y;
            this.z = Vector3.Z;
        }
        public vector3(float X, float Y, float Z)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
        }
        public static explicit operator Vector3(vector3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
        public static explicit operator vector4(vector3 v3)
        {
            return new vector4(v3.x, v3.y, v3.z, 0);
        }
        public static vector3 operator +(vector3 vector0, vector3 vector1)
        {
            return new vector3(vector0.x + vector1.x, vector0.y + vector1.y, vector0.z + vector1.z);
        }
        public static vector3 operator *(vector3 vector0, vector3 vector1)
        {
            return new vector3(vector0.x * vector1.x, vector0.y * vector1.y, vector0.z + vector1.z);
        }
        public static vector3 operator -(vector3 vector0, vector3 vector1)
        {
            return new vector3(vector0.x - vector1.x, vector0.y - vector1.y, vector0.z + vector1.z);
        }
        public static vector3 operator /(vector3 vector0, vector3 vector1)
        {
            return new vector3(vector0.x / vector1.x, vector0.y / vector1.y, vector0.z + vector1.z);
        }
        public static vector3 operator +(vector3 vector0, float float1)
        {
            return new vector3(vector0.x + float1, vector0.y + float1, vector0.z + float1);
        }
        public static vector3 operator *(vector3 vector0, float float1)
        {
            return new vector3(vector0.x * float1, vector0.y * float1, vector0.z + float1);
        }
        public static vector3 operator -(vector3 vector0, float float1)
        {
            return new vector3(vector0.x - float1, vector0.y - float1, vector0.z + float1);
        }
        public static vector3 operator /(vector3 vector0, float float1)
        {
            return new vector3(vector0.x / float1, vector0.y / float1, vector0.z + float1);
        }
    }
    public class vector4 : vector
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float w = 0;
        public vector4()
        {

        }
        public vector4(Vector4 Vector4)
        {
            this.x = Vector4.X;
            this.y = Vector4.Y;
            this.z = Vector4.Z;
            this.w = Vector4.W;
        }
        public vector4(float X, float Y, float Z, float W)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            this.w = W;
        }
        public static explicit operator Vector4(vector4 v4)
        {
            return new Vector4(v4.x, v4.y, v4.z, v4.w);
        }
    }
    public class Camera
    {
        public float NoFogDistance;
        public float CompletelyFogDistance;
        public Vector3 SkyColor;
        private Matrix4 xRot
        {
            get
            {
                return Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-EulerAngles.X));
            }
        }
        private Matrix4 yRot
        {
            get
            {
                return Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-EulerAngles.Y));
            }
        }
        private Matrix4 zRot
        {
            get
            {
                return Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-EulerAngles.Z));
            }
        }
        private Matrix4 Translation
        {
            get
            {
                return Matrix4.CreateTranslation(-Position.X, -Position.Y, -Position.Z);
            }
        }
        public Vector3 Position;
        public Vector3 EulerAngles;
        Vector3 cameraTarget = Vector3.Zero;
        public Vector3 Front
        {
            get
            {
                float x = (float)Math.Cos(MathHelper.DegreesToRadians(EulerAngles.X)) * (float)Math.Cos(MathHelper.DegreesToRadians(EulerAngles.Y));
                float y = (float)Math.Sin(MathHelper.DegreesToRadians(EulerAngles.X));
                float z = (float)Math.Cos(MathHelper.DegreesToRadians(EulerAngles.X)) * (float)Math.Sin(MathHelper.DegreesToRadians(EulerAngles.Y));
                Vector3 FNN = new Vector3(x, y, z);
                FNN = Vector3.Normalize(FNN);
                return FNN;
            }
        }
        Vector3 forward
        {
            get
            {
                return Vector3.Normalize(Position - cameraTarget);
            }
        }
        public Vector3 Right
        {
            get
            {
                return Vector3.Normalize(Vector3.Cross(this.Front, this.up));
            }
        }

        public Vector3 up = new Vector3(0, 1, 0);
        public Vector3 fRight
        {
            get
            {
                return Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            }
        }
        public Vector3 Up
        {
            get
            {
                return Vector3.Normalize(Vector3.Cross(fRight, Front));
            }
        }
        public Matrix4 view
        {
            //get
            //{
            //    return Matrix4.LookAt(Position, Position + Front, up);
            //}
            get
            {
                return zRot * Matrix4.LookAt(Position, Position + this.Front, this.Up);
            }
        }
        public Camera(Vector3 Position, Vector3 EulerAngles, Vector3 SkyColor)
        {
            this.Position = Position;
            this.EulerAngles = EulerAngles;
            this.SkyColor = SkyColor;
            PolyDitchGameEngine.Cameras.Add(this);
        }
    }
    public class Texture
    {
        int TextureHandle;
        Bitmap image;
        public int GetTextureHandle()
        {
            return TextureHandle;
        }
        public Texture(string TexturePath)
        {
            TextureHandle = GL.GenTexture();
            image = new Bitmap(TexturePath);
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            image.UnlockBits(data);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            float[] borderColor = { 1.0f, 1.0f, 0.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -0.4f);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static Texture[] CreateMultipeTextures(params string[] ImageLocations)
        {
            Texture[] Textures = new Texture[ImageLocations.Length];
            for(int i = 0; i < ImageLocations.Length; i++)
            {
                Textures[i] = new Texture(ImageLocations[i]);
            }
            return Textures;
        }
        public void UseTexture(TextureUnit TextureUnit)
        {
            GL.ActiveTexture(TextureUnit);
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
        }
    }
    public class GlobalLight
    {
        public Vector3 Position;
        public Vector3 Color;
        
        public GlobalLight(Vector3 Position, Vector3 Color)
        {
            this.Position = Position;
            this.Color = Color;
        }
    }
    public class GameObject
    {
        public bool Enabled = true;
        public vector3 Position;
        public vector3 LocalPosition
        {
            get
            {
                if(Parent != null)
                {
                    return Parent.Position + localPosition;
                }
                return Position;
            }
            set
            {
                localPosition = value;
            }
        }
        public vector3 EulerAngles;
        public List<PolyDitch> Scripts = new List<PolyDitch>(0);
        public GameObject Parent = null;
        private vector3 localPosition = new vector3();
        public List<GameObject> Children = new List<GameObject>(0);
        public void AddScript(PolyDitch Script)
        {
            Scripts.Add(Script);
            Script.gameObject = this;
        }
        public PolyDitch GetScript<PolyDitch>()
        {
            if (this.HasScript<PolyDitch>())
            {
                return Scripts.OfType<PolyDitch>().ToArray()[0];
            }
            throw new Exception("No scripts of that type exist.");
        }
        public PolyDitch GetScript<PolyDitch>(int index)
        {
            if (this.HasScript<PolyDitch>())
            {
                return Scripts.OfType<PolyDitch>().ToArray()[index];
            }
            throw new Exception("No scripts of that type exist.");
        }
        public bool HasScript<PolyDitch>()
        {
            if(Scripts.OfType<PolyDitch>().ToArray().Length > 0)
            {
                return true;
            }
            return false;
        }
        public GameObject(vector3 Position, vector3 EulerAngles)
        {
            this.Position = Position;
            this.EulerAngles = EulerAngles;
            PolyDitchGameEngine.AddGameObject(this);
        }
        public GameObject(vector3 Position, vector3 EulerAngles, GameObject Parent)
        {
            this.Position = Position;
            this.EulerAngles = EulerAngles;
            this.Parent = Parent;
            PolyDitchGameEngine.AddGameObject(this);
        }
    }
    public class PolyDitch
    {
        /// <summary>
        /// The gameObject this script is attactched to.
        /// </summary>
        public GameObject gameObject;
        /// <summary>
        /// This method will run when the program starts if the gameObject this script is attatched to is enabled.
        /// </summary>
        public virtual void OnStart()
        {

        }
        /// <summary>
        /// This method will be run whenever the gameObject the script is attachted to is enabled.
        /// </summary>
        public virtual void OnEnabled()
        {

        }
        /// <summary>
        /// This method will run every frame (Except for the first) if the gameObject this script is attatched to is enabled.
        /// </summary>
        public virtual void OnUpdate()
        {

        }
    }
    public class MeshRenderer : PolyDitch
    {
        public Mesh Mesh;
        public Material Material;
        public Shader OverrideShader = null;
        public Matrix4 ModelTransformation
        {
            get
            {
                return Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(gameObject.EulerAngles.z)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(gameObject.EulerAngles.y)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(gameObject.EulerAngles.x)) * Matrix4.CreateTranslation(gameObject.Position.y, gameObject.Position.z, gameObject.Position.x);
                //return Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(gameObject.EulerAngles.z));
            }
        }
        public MeshRenderer(Mesh Mesh, Material Material)
        {
            this.Mesh = Mesh;
            this.Material = Material;
            PolyDitchGameEngine.MeshesToRender.Add(this);
            this.Mesh.vao.MeshRenderer = this;
        }
        public MeshRenderer(Mesh Mesh, Material Material, Shader OverrideShader)
        {
            this.Mesh = Mesh;
            this.Material = Material;
            this.OverrideShader = OverrideShader;
            PolyDitchGameEngine.MeshesToRender.Add(this);
            this.Mesh.vao.MeshRenderer = this;
        }
    }
    public class Terrain : PolyDitch
    {
        public float Size = 800;
        public int VertexCount = 128;
        public float MaxHeight = 40;
        public float MinimumHeight = -40;
        public float MaxPixelColor = 256 * 256 * 256;

        public TerrainTexturePack TerrainTexturePack;

        public Material TerrainMaterial;
        public Shader TerrainShader;
        public Mesh TerrainMesh;

        public Terrain(TerrainTexturePack TerrainTexturePack, string HeightMap)
        {
            this.TerrainTexturePack = TerrainTexturePack;
            TerrainTexturePack.HeightMap = HeightMap;
            TerrainShader = new Shader(@"TerrainVertex.glsl", @"TerrainFragment.glsl");
            TerrainMaterial = new Material(TerrainTexturePack, TerrainShader, 0, 10);
            TerrainMesh = generateTerrain(TerrainTexturePack.HeightMap);
            this.TerrainMesh.vao.SpecialInformation = "Terrain";
            this.TerrainMesh.vao.Terrain = this;
        }
        private Mesh generateTerrain(string HeightMap)
        {
            Bitmap image = new Bitmap(HeightMap);
            VertexCount = image.Height;
            int count = VertexCount * VertexCount;
            float[] vertices = new float[count * 3];
            float[] normals = new float[count * 3];
            float[] textureCoords = new float[count * 2];
            uint[] indices = new uint[6 * (VertexCount - 1) * (VertexCount - 1)];
            int vertexPointer = 0;
            for (int i = 0; i < VertexCount; i++)
            {
                for (int j = 0; j < VertexCount; j++)
                {
                    vertices[vertexPointer * 3] = (float)j / ((float)VertexCount - 1) * Size;
                    vertices[vertexPointer * 3 + 1] = GetHeight(j, i, image);
                    vertices[vertexPointer * 3 + 2] = (float)i / ((float)VertexCount - 1) * Size;
                    Vector3 Normal = HeightMapNormals(j, i, image);
                    normals[vertexPointer * 3] = Normal.X;
                    normals[vertexPointer * 3 + 1] = Normal.Y;
                    normals[vertexPointer * 3 + 2] = Normal.Z;
                    textureCoords[vertexPointer * 2] = (float)j / ((float)VertexCount - 1);
                    textureCoords[vertexPointer * 2 + 1] = (float)i / ((float)VertexCount - 1);
                    vertexPointer++;
                }
            }
            int pointer = 0;
            for (int gz = 0; gz < VertexCount - 1; gz++)
            {
                for (int gx = 0; gx < VertexCount - 1; gx++)
                {
                    uint topLeft = (uint)((gz * VertexCount) + gx);
                    uint topRight = topLeft + 1;
                    uint bottomLeft = (uint)(((gz + 1) * VertexCount) + gx);
                    uint bottomRight = bottomLeft + 1;
                    indices[pointer++] = topLeft;
                    indices[pointer++] = bottomLeft;
                    indices[pointer++] = topRight;
                    indices[pointer++] = topRight;
                    indices[pointer++] = bottomLeft;
                    indices[pointer++] = bottomRight;
                }
            }
            return new Mesh(vertices, textureCoords, normals, indices);   
        }
        private float GetHeight(int x, int z, Bitmap Image)
        {
            if (x < 0 || x >= Image.Height || z < 0 || z >= Image.Height)
            {
                return 0;
            }
            Color HeightColor = Image.GetPixel(x, z);
            float Height = HeightColor.R * HeightColor.G * HeightColor.B;
            Height += MaxPixelColor / 2;
            Height /= MaxPixelColor / 2;
            Height *= MaxHeight;
            //Console.WriteLine(Height);
            return Height;
        }
        private Vector3 HeightMapNormals(int x, int z, Bitmap Image)
        {
            float HeightL = GetHeight(x - 1, z, Image);
            float HeightR = GetHeight(x + 1, z, Image);
            float HeightD = GetHeight(x, z - 1, Image);
            float HeightU = GetHeight(x, z + 1, Image);
            Vector3 Normal = new Vector3(HeightL - HeightR, 2f, HeightD - HeightU);
            Normal.Normalize();
            return Normal;
        }
    }
    public class TerrainTexturePack
    {
        public string HeightMap;
        public Texture Texture0;
        public Texture RTexture;
        public Texture GTexture;
        public Texture BTexture;
        public Texture BlendMap;
        public TerrainTexturePack(Texture Texture0, Texture RTexture, Texture GTexture, Texture BTexture, Texture BlendMap)
        {
            this.Texture0 = Texture0;
            this.RTexture = RTexture;
            this.GTexture = GTexture;
            this.BTexture = BTexture;
            this.BlendMap = BlendMap;
        }
        public TerrainTexturePack(Texture[] Textures)
        {
            Console.WriteLine("Debug Splitter Start");
            Console.WriteLine(Textures[0].GetTextureHandle());
            Console.WriteLine("Debug Splitter End");
            this.Texture0 = Textures[0];
            this.RTexture = Textures[1];
            this.GTexture = Textures[2];
            this.BTexture = Textures[3];
            this.BlendMap = Textures[4];
        }
    }
    public static class Input
    {
        public static KeyboardState KeyState;
        public static void Start()
        {

        }
        public static void Update()
        {
            KeyState = Keyboard.GetState();
        }
        public static bool IsKeyDown(Key key)
        {
            if (KeyState.IsKeyDown(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public static class Time
    {
        public static float DeltaTime
        {
            get
            {
                return deltaTime * TimeScale;
            }
        }
        private static float deltaTime;
        public static float TimeScale = 1;
        public static void Update(double GLDeltaTime)
        {
            deltaTime = (float)GLDeltaTime;
        }
    }
    public class BasicMovementScript : PolyDitch
    {
        public Camera camera;
        public float speed = 15;

        public override void OnUpdate()
        {
            if (Input.IsKeyDown(Key.W))
            {
                camera.Position += camera.Front * speed * Time.DeltaTime;
            }
            if (Input.IsKeyDown(Key.S))
            {
                camera.Position -= camera.Front * speed * Time.DeltaTime;
            }
            if (Input.IsKeyDown(Key.A))
            {
                camera.Position -= camera.Right * speed * Time.DeltaTime;
            }
            if (Input.IsKeyDown(Key.D))
            {
                camera.Position += camera.Right * speed * Time.DeltaTime;
            }
            if (Input.IsKeyDown(Key.Space))
            {
                camera.Position += camera.up * speed * Time.DeltaTime; //Up 
            }
            if (Input.IsKeyDown(Key.LShift))
            {
                camera.Position -= camera.up * speed * Time.DeltaTime; //Down 
            }

            if (Input.IsKeyDown(Key.Right))
            {
                camera.EulerAngles.Y += speed * 5 * Time.DeltaTime; //Down 
            }
            if (Input.IsKeyDown(Key.Left))
            {
                camera.EulerAngles.Y -= speed * 5 * Time.DeltaTime; //Down 
            }
            if (Input.IsKeyDown(Key.Up))
            {
                camera.EulerAngles.X -= speed * 5 * Time.DeltaTime; //Down 
            }
            if (Input.IsKeyDown(Key.Down))
            {
                camera.EulerAngles.X += speed * 5 * Time.DeltaTime; //Down 
            }
        }
    }
}
