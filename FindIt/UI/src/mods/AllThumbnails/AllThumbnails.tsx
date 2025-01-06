import React, { useEffect, useState } from "react";
import { useValue, bindValue, trigger } from "cs2/api";
import mod from "../../../mod.json";
import styles from "./AllThumbnails.module.scss";

const AllThumbnails$ = bindValue<string[]>(mod.id, "AllThumbnails");

export const AllThumbnailsComponent = () => {
  const AllThumbnails = useValue(AllThumbnails$);
  const batchSize = 200;
  const [loaded, setLoaded] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setLoaded((prevLoaded) => {
        const newLoaded = prevLoaded + batchSize;
        if (newLoaded >= AllThumbnails.length) {
          clearInterval(interval); // Stop the interval when limit is reached
        }
        return Math.min(newLoaded, AllThumbnails.length); // Ensure we don't load more than available
      });
    }, 400);

    return () => clearInterval(interval); // Cleanup interval on unmount
  }, [AllThumbnails.length, batchSize]);

  if (loaded >= AllThumbnails.length) {
    trigger(mod.id, "ClearThumbnails");
    console.log("Caching thumbnails completed");
    return <></>;
  }

  console.log("Caching thumbnails: " + loaded + " / " + AllThumbnails.length);

  return (
    <div className={styles.container}>
      {AllThumbnails.slice(loaded, loaded + batchSize).map((x, index) => (
        <img
          key={index}
          src={x}
          onError={({ currentTarget }) => {
            currentTarget.onerror = null; // prevents looping
          }}
        />
      ))}
    </div>
  );
};
