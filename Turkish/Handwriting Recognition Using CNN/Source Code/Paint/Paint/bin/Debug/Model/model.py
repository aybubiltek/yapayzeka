from keras.datasets import mnist
from matplotlib import pyplot as plt
import cv2
import time
import numpy as np
from keras.layers import Dense, Conv2D, MaxPooling2D, Flatten, Dropout
from keras.models import Sequential, load_model
from keras.utils import to_categorical


(train_images,train_labels),(test_images,test_labels)=mnist.load_data()
train_images=train_images.reshape(-1,28,28,1)
test_images=test_images.reshape(-1,28,28,1)
train_images=train_images.astype('float32')/255.0
test_images=test_images.astype('float32')/255.0
print(train_labels)
train_labels=to_categorical(train_labels)
print(train_labels)
test_labels=to_categorical(test_labels)

model=Sequential()
model.add(Conv2D(32,
                 (3,3),
                 activation='relu',
                 input_shape=(28,28,1)))
model.add(Conv2D(32,
                 (3,3),
                 activation='relu'))
model.add(MaxPooling2D(2,2))
model.add(Flatten())
model.add(Dense(64,
                activation='relu'))
model.add(Dense(10,
                activation='softmax'))
model.compile(optimizer='rmsprop',
              loss='categorical_crossentropy',
              metrics=['accuracy'])

"""model.fit(train_images,
          train_labels,
          batch_size=64,
          epochs=15,
          verbose=1,
          validation_split=0.2)
model.save('model_new.h5')"""





model=load_model('model_new.h5')
while True:
    try:
        dosya_yolu = 'tahmin'
        img = cv2.imread("valid_images/"+dosya_yolu+".png", 0)
        img = np.array([img]).reshape(-1, 28, 28, 1)
        prediction = model.predict(img)
        count = 0
        for i in prediction[0]:
            if i == 1.0:
                break
            else:
                count += 1
        if count==10:
            count='-'

        print(count)
        with open("valid_images/tahmin.txt", "w") as f:
            f.write(str(count))
        time.sleep(0.5)
    except:
        pass

