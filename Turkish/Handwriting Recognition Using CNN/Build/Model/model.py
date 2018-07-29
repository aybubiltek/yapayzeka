#Gerekli kütüphanelerimizi içeri aktarıyoruz.
from keras.datasets import mnist 
import cv2 
import time 
import numpy as np
from keras.layers import Dense, Conv2D, MaxPooling2D, Flatten, Dropout
from keras.models import Sequential, load_model
from keras.utils import to_categorical

#Eğitim ve test verilerimizi aktarıyoruz.
(train_images,train_labels),(test_images,test_labels)=mnist.load_data()

#Değişken boyutlarını düzenliyoruz.
train_images=train_images.reshape(-1,28,28,1)
test_images=test_images.reshape(-1,28,28,1)
train_images=train_images.astype('float32')/255.0
test_images=test_images.astype('float32')/255.0
train_labels=to_categorical(train_labels)
test_labels=to_categorical(test_labels)

#Model yapısını oluşturuyoruz, bu modelde 4 katmanlı CNN yapısı kullanıldı.
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


#Eğitim sonrası modelimizin ağırlıklarını dahil ediyoruz.
model=load_model('model_new.h5')

#Tahmin işlemimizi gerçekleştireceğimiz döngümüz burası.
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

        f=open("valid_images/tahmin.txt","w")
        count=str(count)
        f.write(count)
        f.close()
        time.sleep(0.5)
    except Exception as error:
        pass

