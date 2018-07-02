ALTER TABLE Containers
ADD FOREIGN KEY (TripId) REFERENCES Trips(Id);

ALTER TABLE [Temperature Records]
ADD FOREIGN KEY (ContainerId) REFERENCES Containers(Id);


