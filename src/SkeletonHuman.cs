using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Ageless {
    public class SkeletonHuman : Skeleton{



        Bone torso;
        Bone head;
        Bone legUpperLeft;
        Bone legUpperRight;
        Bone legLowerLeft;
        Bone legLowerRight;
        Bone armUpperLeft;
        Bone armUpperRight;
        Bone armLowerLeft;
        Bone armLowerRight;
        Bone footLeft;
        Bone footRight;

        public const int ANIM_STAND = 0;
        public const int ANIM_RUN = 1;


        public SkeletonHuman() {

            torso = new Bone(null, ModelControl.getModel("human/torso"), 0);
            head = new Bone(torso, ModelControl.getModel("human/head"), 0);

            legUpperLeft = new Bone(torso, ModelControl.getModel("human/upperLeg"), 0);
            legLowerLeft = new Bone(legUpperLeft, ModelControl.getModel("human/lowerLeg"), 0);
            legUpperRight = new Bone(torso, ModelControl.getModel("human/upperLeg"), 0);
            legLowerRight = new Bone(legUpperRight, ModelControl.getModel("human/lowerLeg"), 0);

            armUpperLeft = new Bone(torso, ModelControl.getModel("human/upperArm"), 0);
            armLowerLeft = new Bone(armUpperLeft, ModelControl.getModel("human/lowerArm"), 0);
            armUpperRight = new Bone(torso, ModelControl.getModel("human/upperArm"), 0);
            armLowerRight = new Bone(armUpperRight, ModelControl.getModel("human/lowerArm"), 0);

            footLeft = new Bone(legLowerLeft, ModelControl.getModel("human/foot"), 0);
            footRight = new Bone(legLowerRight, ModelControl.getModel("human/foot"), 0);

            updateMatrices();


            bones.Add(torso);
            bones.Add(head);
            bones.Add(legUpperLeft);
            bones.Add(legLowerLeft);
            bones.Add(legUpperRight);
            bones.Add(legLowerRight);
            bones.Add(armUpperLeft);
            bones.Add(armLowerLeft);
            bones.Add(armUpperRight);
            bones.Add(armLowerRight);
            bones.Add(footLeft);
            bones.Add(footRight);
        }


        //Torso
        public float animTorsoRZ() {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * -0.05);
            } else {
                return 0;
            }
        }

        //Head
        public float animHeadRZ() {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * 0.05);
            } else {
                return 0;
            }
        }

        //Legs
        public float animUpperLegRZ(bool left) {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * (left ? 1 : -1) * Math.Cos(animTick / 7.0) * -0.2 + Math.PI);
            } else {
                return (float)(Math.PI);
            }
        }
        public float animLowerLegRZ(bool left) {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * (left ? 1 : -1) * Math.Sin(animTick / 7.0) * -0.1 + Math.PI * -0.2);
            } else {
                return 0;
            }
        }

        //Foot
        public float animFootRZ(bool left) {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * (left ? 1 : -1) * Math.Sin(animTick / 7.0) * -0.1 + Math.PI);
            } else {
                return (float)(Math.PI);
            }
        }

        //Arms
        public float animUpperArmRZ(bool left) {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * (left ? 1 : -1) * Math.Sin(animTick / 7.0) * -0.2 + Math.PI);
            } else {
                return (float)(Math.PI);
            }
        }
        public float animLowerArmRZ(bool left) {
            if (animation == ANIM_RUN) {
                return (float)(Math.PI * (left ? 1 : -1) * Math.Cos(animTick / 7.0) * -0.1 + Math.PI * 0.2);
            } else {
                return 0;
            }
        }

        public override void updateMatrices() {

            const int size = 3;

            torso.matrix = Matrix4.CreateRotationZ(animTorsoRZ()) * Matrix4.CreateTranslation(0, size * .43f, 0);
            head.matrix = Matrix4.CreateRotationZ(animHeadRZ()) * Matrix4.CreateTranslation(0, size * .38f, 0);

            legUpperLeft.matrix = Matrix4.CreateRotationZ(animUpperLegRZ(true)) * Matrix4.CreateTranslation(0, 0, size * .055f);
            legUpperRight.matrix = Matrix4.CreateRotationZ(animUpperLegRZ(false)) * Matrix4.CreateTranslation(0, 0, size * -.055f);
            legLowerLeft.matrix = Matrix4.CreateRotationZ(animLowerLegRZ(true)) * Matrix4.CreateTranslation(0, size * .17f, 0);
            legLowerRight.matrix = Matrix4.CreateRotationZ(animLowerLegRZ(false)) * Matrix4.CreateTranslation(0, size * .17f, 0);

            footLeft.matrix = Matrix4.CreateRotationZ(animFootRZ(true)) * Matrix4.CreateTranslation(0, size * .22f, 0);
            footRight.matrix = Matrix4.CreateRotationZ(animFootRZ(false)) * Matrix4.CreateTranslation(0, size * .22f, 0);

            armUpperLeft.matrix = Matrix4.CreateRotationZ(animUpperArmRZ(true)) * Matrix4.CreateTranslation(0, size * .38f, size * .26f * .5f);
            armUpperRight.matrix = Matrix4.CreateRotationZ(animUpperArmRZ(false)) * Matrix4.CreateTranslation(0, size * .38f, size * -.26f * .5f);
            armLowerLeft.matrix = Matrix4.CreateRotationZ(animLowerArmRZ(true)) * Matrix4.CreateTranslation(0, size * .18f, 0);
            armLowerRight.matrix = Matrix4.CreateRotationZ(animLowerArmRZ(false)) * Matrix4.CreateTranslation(0, size * .18f, 0);
        }

    }
}
