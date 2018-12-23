using Android.Content;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using JP.Live2d.Android;
using System;

namespace GFI_with_GFS_A
{
    public class Live2DSurfaceView : GLSurfaceView
    {
        private Live2DGLRenderer renderer;

        public Live2DSurfaceView(Context context, string ModelPath, string[] TexturePath) : base(context)
        {
            renderer = new Live2DGLRenderer(ModelPath, TexturePath);
            SetRenderer(renderer);
        }
    }

    public class Live2DGLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private Live2DModelAndroid live2DModel;
        private string ModelPath = "";
        private string[] TexturePath;

        public void Dispose() { }

        public Live2DGLRenderer(string ModelPath, string[] TexturePath)
        {
            this.ModelPath = ModelPath;
            this.TexturePath = TexturePath;
        }

        public void OnDrawFrame(IGL10 gl)
        {
            gl.GlMatrixMode(GL10.GlModelview);
            gl.GlLoadIdentity();
            gl.GlClear(GL10.GlColorBufferBit);
            gl.GlEnable(GL10.GlBlend);
            gl.GlBlendFunc(GL10.GlOne, GL10.GlOneMinusSrcAlpha);
            gl.GlDisable(GL10.GlDepthTest);
            gl.GlDisable(GL10.GlCullFaceCapability);

            live2DModel.SetGL(gl);
            live2DModel.Update();
            live2DModel.Draw();
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            gl.GlViewport(0, 0, width, height);
            gl.GlMatrixMode(GL10.GlProjection);
            gl.GlLoadIdentity();

            float ModelWidth = live2DModel.CanvasWidth;
            float VisibleWidth = ModelWidth * (3.0f / 4.0f);
            float Margin = 0.5f * (ModelWidth / 4.0f);

            gl.GlOrthof(Margin, Margin + VisibleWidth, VisibleWidth * height / width, 0, 0.5f, -0.5f);
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            live2DModel = Live2DModelAndroid.LoadModel(ModelPath);
        }
    }
}