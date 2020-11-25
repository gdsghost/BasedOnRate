using System;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using WoWonder.NiceArt.Models;
using Exception = System.Exception;

namespace WoWonder.NiceArt
{
    public class MultiTouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        public static readonly int InvalidPointerId = -1;
        public readonly GestureDetector MGestureListener;
        public static bool IsRotateEnabled = true;
        public static bool IsTranslateEnabled = true;
        public static bool IsScaleEnabled = true;
        public static float MinimumScale = 0.5f;
        public static float MaximumScale = 10.0f;
        public int MActivePointerId = InvalidPointerId;
        public float MPrevX, MPrevY, MPrevRawX, MPrevRawY;
        public ScaleGestureDetector MScaleGestureDetector;

        public int[] Location = new int[2];
        public Rect OutRect;
        public View DeleteView;
        public ImageView PhotoEditImageView;
        public RelativeLayout ParentView;

        public INiceArt.IOnMultiTouchListener OnMultiTouchListener;
        public static INiceArt.IOnGestureControl MOnGestureControl;
        public static bool MIsTextPinchZoomable;
        public INiceArt.IOnNiceArtEditorListener MOnNiceArtEditorListener;
        public static ViewType MviewType;

        public MultiTouchListener(View deleteView, RelativeLayout parentView, ImageView photoEditImageView, bool isTextPinchZoomable, INiceArt.IOnNiceArtEditorListener onNiceArtEditorListener)
        {
            try
            {
                MIsTextPinchZoomable = isTextPinchZoomable;
                MScaleGestureDetector = new ScaleGestureDetector(new ScaleGestureListener());

                MGestureListener = new GestureDetector(Application.Context, new GestureListener());
                DeleteView = deleteView;
                ParentView = parentView;
                PhotoEditImageView = photoEditImageView;
                MOnNiceArtEditorListener = onNiceArtEditorListener;
                OutRect = deleteView != null ? new Rect(deleteView.Left, deleteView.Top, deleteView.Right, deleteView.Bottom) : new Rect(0, 0, 0, 0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static float AdjustAngle(float degrees)
        {
            try
            {
                if (degrees > 180.0f)
                {
                    degrees -= 360.0f;
                }
                else if (degrees < -180.0f)
                {
                    degrees += 360.0f;
                }

                return degrees;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;

            }
        }

        public static void Move(View view, TransformInfo info)
        {
            try
            {
                ComputeRenderOffset(view, info.pivotX, info.pivotY);
                AdjustTranslation(view, info.deltaX, info.deltaY);

                float scale = view.ScaleX * info.deltaScale;
                scale = Math.Max(info.minimumScale, Math.Min(info.maximumScale, scale));
                view.ScaleX = scale;
                view.ScaleY = scale;

                float rotation = AdjustAngle(view.Rotation + info.deltaAngle);
                view.Rotation = rotation;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static void AdjustTranslation(View view, float deltaX, float deltaY)
        {
            try
            {
                float[] deltaVector = { deltaX, deltaY };
                view.Matrix.MapVectors(deltaVector);
                view.TranslationX = view.TranslationX + deltaVector[0];
                view.TranslationY = view.TranslationY + deltaVector[1];
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public static void ComputeRenderOffset(View view, float pivotX, float pivotY)
        {
            try
            {
                if (view.PivotX == pivotX && view.PivotY == pivotY)
                {
                    return;
                }

                float[] prevPoint = { 0.0f, 0.0f };
                view.Matrix.MapPoints(prevPoint);

                view.PivotX = pivotX;
                view.PivotY = pivotY;

                float[] currPoint = { 0.0f, 0.0f };
                view.Matrix.MapPoints(currPoint);

                float offsetX = currPoint[0] - prevPoint[0];
                float offsetY = currPoint[1] - prevPoint[1];

                view.TranslationX = view.TranslationX - offsetX;
                view.TranslationY = view.TranslationY - offsetY;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public bool OnTouch(View v, MotionEvent Event)
        {
            try
            {
                MScaleGestureDetector.OnTouchEvent(v, Event);
                MGestureListener.OnTouchEvent(Event);

                if (!IsTranslateEnabled)
                {
                    return true;
                }

                var action = Event.Action;

                int x = (int)Event.RawX;
                int y = (int)Event.RawY;

                switch (action & Event.ActionMasked)
                {
                    case MotionEventActions.Down:
                        MPrevX = Event.GetX();
                        MPrevY = Event.GetY();
                        MPrevRawX = Event.RawX;
                        MPrevRawY = Event.RawY;
                        MActivePointerId = Event.GetPointerId(0);
                        if (DeleteView != null)
                        {
                            DeleteView.Visibility = ViewStates.Visible;
                        }
                        v.BringToFront();
                        FireNiceArtEditorSdkListener(v, true);
                        break;

                    case MotionEventActions.Move:
                        int pointerIndexMove = Event.FindPointerIndex(MActivePointerId);
                        if (pointerIndexMove != -1)
                        {
                            float currX = Event.GetX(pointerIndexMove);
                            float currY = Event.GetY(pointerIndexMove);
                            if (!MScaleGestureDetector.IsInProgress())
                            {
                                AdjustTranslation(v, currX - MPrevX, currY - MPrevY);
                            }
                        }
                        break;

                    case MotionEventActions.Cancel:
                        MActivePointerId = InvalidPointerId;
                        break;

                    case MotionEventActions.Up:
                        MActivePointerId = InvalidPointerId;
                        if (DeleteView != null && IsViewInBounds(DeleteView, x, y))
                        {
                            OnMultiTouchListener?.OnRemoveViewListener(v);
                        }
                        else if (!IsViewInBounds(PhotoEditImageView, x, y))
                        {
                            //v.Animate().TranslationY(0).TranslationY(0);
                        }
                        if (DeleteView != null)
                        {
                            DeleteView.Visibility = ViewStates.Gone;
                        }
                        FireNiceArtEditorSdkListener(v, false);
                        break;

                    case MotionEventActions.PointerUp:

                        int pointerIndex = (int)(Event.Action & MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                        int pointerId = Event.GetPointerId(pointerIndex);
                        if (pointerId == MActivePointerId)
                        {
                            int newPointerIndex = pointerId == 0 ? 1 : 0;
                            MPrevX = Event.GetX(newPointerIndex);
                            MPrevY = Event.GetY(newPointerIndex);
                            MActivePointerId = Event.GetPointerId(newPointerIndex);
                        }
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void FireNiceArtEditorSdkListener(View view, bool isStart)
        {
            try
            {
                if (view is TextView)
                {
                    if (OnMultiTouchListener != null)
                    {
                        if (MOnNiceArtEditorListener != null)
                        {
                            if (isStart)
                                MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Text);
                            else
                                MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Text);
                        }
                    }
                    else
                    {
                        if (MOnNiceArtEditorListener != null)
                        {
                            if (isStart)
                                MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Emojis);
                            else
                                MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Emojis);
                        }
                    }
                }
                else
                {
                    if (MOnNiceArtEditorListener != null)
                    {
                        if (isStart)
                            MOnNiceArtEditorListener.OnStartViewChangeListener(ViewType.Image);
                        else
                            MOnNiceArtEditorListener.OnStopViewChangeListener(ViewType.Image);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public bool IsViewInBounds(View view, int x, int y)
        {
            try
            {
                view.GetDrawingRect(OutRect);
                view.GetLocationOnScreen(Location);
                OutRect.Offset(Location[0], Location[1]);
                return OutRect.Contains(x, y);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public void SetOnMultiTouchListener(INiceArt.IOnMultiTouchListener onMultiTouchListener)
        {
            try
            {
                OnMultiTouchListener = onMultiTouchListener;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public class ScaleGestureListener : INiceArt.IOnScaleGestureListener
        {
            public float mPivotX;
            public float mPivotY;
            public Vector2D mPrevSpanVector = new Vector2D();

            public ScaleGestureDetector mScaleGestureDetector;
            public float mScaleFactor = 1.0f;
            public ImageView mImageView;

            public bool OnScale(View view, ScaleGestureDetector detector)
            {
                try
                {
                    //Just Zoom >>
                    //=====================
                    //mScaleFactor *= detector.GetScaleFactor();
                    //mScaleFactor = Math.Max(0.1f, Math.Min(mScaleFactor, 10.0f));
                    //view.ScaleX = mScaleFactor;
                    //view.ScaleY = mScaleFactor;

                    //return true;

                    // Zoom and Rotate >>
                    //=====================
                    TransformInfo info = new TransformInfo();
                    info.deltaScale = IsScaleEnabled ? detector.GetScaleFactor() : 1.0f;
                    info.deltaAngle = IsRotateEnabled ? Vector2D.GetAngle(mPrevSpanVector, detector.GetCurrentSpanVector()) : 0.0f;
                    info.deltaX = IsTranslateEnabled ? detector.GetFocusX() - mPivotX : 0.0f;
                    info.deltaY = IsTranslateEnabled ? detector.GetFocusY() - mPivotY : 0.0f;
                    info.pivotX = mPivotX;
                    info.pivotY = mPivotY;
                    info.minimumScale = MinimumScale;
                    info.maximumScale = MaximumScale;
                    Move(view, info);
                    return !MIsTextPinchZoomable;

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;

                }
            }

            public bool OnScaleBegin(View view, ScaleGestureDetector detector)
            {
                try
                {
                    mPivotX = detector.GetFocusX();
                    mPivotY = detector.GetFocusY();
                    mPrevSpanVector.Set(detector.GetCurrentSpanVector());
                    return MIsTextPinchZoomable;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;

                }
            }

            public void OnScaleEnd(View view, ScaleGestureDetector detector)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public class TransformInfo
        {
            public float deltaX;
            public float deltaY;
            public float deltaScale;
            public float deltaAngle;
            public float pivotX;
            public float pivotY;
            public float minimumScale;
            public float maximumScale;
        }

        public void SetOnGestureControl(INiceArt.IOnGestureControl onGestureControl, ViewType viewType)
        {
            try
            {
                MviewType = viewType;
                MOnGestureControl = onGestureControl;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

            }
        }

        public class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            public override bool OnSingleTapUp(MotionEvent e)
            {
                try
                {
                    MOnGestureControl?.OnClick();
                    return true;
                }
                catch (Exception ex)
                {
                   Methods.DisplayReportResultTrack(ex);
                    return false;
                }
            }

            public override void OnLongPress(MotionEvent e)
            {
                try
                {
                    base.OnLongPress(e);
                    if (MviewType == ViewType.Text)
                    {
                        // mOnGestureControl?.OnLongClick(mviewType);
                    }
                }
                catch (Exception ex)
                {
                   Methods.DisplayReportResultTrack(ex);
                }
            }


            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                return true;
            }

        }
    }
}