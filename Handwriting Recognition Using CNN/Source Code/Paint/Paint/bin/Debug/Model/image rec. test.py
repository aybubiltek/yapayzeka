from keras.models import Sequential
import cv2
from keras.layers import Dense, Conv2D, MaxPooling2D, Flatten
import keras
import numpy as np
from keras.preprocessing.image import ImageDataGenerator

def input_dir(main_input_dir):
    import os
    folders = 0
    for _, dirnames, _ in os.walk(main_input_dir):
        folders += len(dirnames)
    train_images=np.zeros((folders,750))
    for (subdirs, dirs, files) in os.walk(main_input_dir):
        for subdir in dirs:
            subjectpath = os.path.join(main_input_dir, subdir)
            for filename in os.listdir(subjectpath):
                path = subjectpath + '/' + filename
                image=cv2.imread(path)
                np.append()




output_classes=2
input_shape=(200,200,1)
model = Sequential()
model.add(Conv2D(32,kernel_size=(5, 5), strides=(1, 1), activation='relu', input_shape=input_shape))
model.add(MaxPooling2D(pool_size=(2, 2), strides=(2, 2)))
model.add(Conv2D(64, kernel_size=(5, 5), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Flatten())
model.add(Dense(1000, activation='relu'))
model.add(Dense(output_classes, activation='softmax'))
model.compile(optimizer=keras.optimizers.sgd(lr=(0.01)),loss=keras.losses.categorical_crossentropy,metrics=['accuracy'])
train_datagen = ImageDataGenerator(rescale = 1./255,
                                   shear_range = 0.2,
                                   zoom_range = 0.2,
                                   horizontal_flip = True)
training_set = train_datagen.flow_from_directory("C:\\Users\\burak\\Desktop\\Yapay zeka projeleri\\test and valid\\carsdetected",
                                                 target_size = (200,200),
                                                 batch_size = 32,
                                                 class_mode = 'binary')
test_datagen = ImageDataGenerator(rescale = 1./255)
test_set = test_datagen.flow_from_directory("C:\\Users\\burak\\Desktop\\Yapay zeka projeleri\\test and valid\\carsdetected_test",
                                            target_size = (64, 64),
                                            batch_size = 32,
                                            class_mode = 'binary')

"""x_train = np.array(training_set.seed)
y_train = np.array(training_set.classes)
model.fit(x_train,
          y_train,
          batch_size=64,
          epochs=15,
          verbose=1,
          validation_split=0.2)
model.save('model_new.h5')"""

model.fit_generator(training_set.,
                    steps_per_epoch = 8000,
                    epochs = 25,
                    validation_data = test_set,
                    validation_steps = 2000)
model.save("model.new.h5")