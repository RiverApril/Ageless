﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Ageless {
    public class RenderMaker {

        private Thread thread = null;

        public List<Renderable> list = new List<Renderable>();

        internal void initRenderMaker() {
            if (thread != null) {
                thread.Join();
                thread = null;
            }
            thread = new Thread(compilerRenders);
            thread.Start();
        }
        
        private void compilerRenders() {
            while (!Game.exiting) {
                while (!Game.exiting && list.Count > 0) {
                    list[0].makeRender();
                    list.RemoveAt(0);
                }
            }
        }
    }
}