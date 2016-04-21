using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Ageless {
    class TryGL { //Copied from http://www.opentk.com/node/3691
        public static ErrorCode LastError { get; private set; }

        /// <summary>
        /// Call OpenGL function and check for the error
        /// </summary>
        /// <param name="callback">OpenGL Function to be called</param>
        public static void Call(Action callback) {
            callback();
            CheckError();
        }

        /// <summary>
        /// OpenGL function and check for the error
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">OpenGL Function to be called</param>
        /// <param name="parameter">Parameters of OpenGL Function</param>
        public static void Call<T>(Action<T> func, T parameter) {
            func(parameter);
            CheckError();
        }

        /// <summary>
        /// Check for the OpenGL Error
        /// </summary>
        public static void CheckError() {
            ErrorCode errorCode = GL.GetError();

            if (errorCode == ErrorCode.NoError)
                return;

            LastError = errorCode;
            string error = "Unknown Error";
            string description = "No Description";

            // Decode the error code
            switch (errorCode) {
                case ErrorCode.InvalidEnum: {
                        error = "GL_INVALID_ENUM";
                        description = "An unacceptable value has been specified for an enumerated argument";
                        break;
                    }

                case ErrorCode.InvalidValue: {
                        error = "GL_INVALID_VALUE";
                        description = "A numeric argument is out of range";
                        break;
                    }

                case ErrorCode.InvalidOperation: {
                        error = "GL_INVALID_OPERATION";
                        description = "The specified operation is not allowed in the current state";
                        break;
                    }

                case ErrorCode.StackOverflow: {
                        error = "GL_STACK_OVERFLOW";
                        description = "This command would cause a stack overflow";
                        break;
                    }

                case ErrorCode.StackUnderflow: {
                        error = "GL_STACK_UNDERFLOW";
                        description = "This command would cause a stack underflow";
                        break;
                    }

                case ErrorCode.OutOfMemory: {
                        error = "GL_OUT_OF_MEMORY";
                        description = "there is not enough memory left to execute the command";
                        break;
                    }

                case ErrorCode.InvalidFramebufferOperationExt: {
                        error = "GL_INVALID_FRAMEBUFFER_OPERATION_EXT";
                        description = "The object bound to FRAMEBUFFER_BINDING_EXT is not \"framebuffer complete\"";
                        break;
                    }
                default: {
                        error = errorCode.ToString();
                        break;
                    }
            }

            // Log the error
            Console.WriteLine("An internal OpenGL call failed: " + error + " (" + description + ")", "Fatal Error");
            throw new Exception();
        }
    }
}
