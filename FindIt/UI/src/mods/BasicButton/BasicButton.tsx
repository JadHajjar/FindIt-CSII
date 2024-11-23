import { Button, Tooltip } from "cs2/ui";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver/VanillaComponentResolver";
import { PropsWithChildren } from "react";

type _Props = {
  tooltip?: string | null;
  selected?: boolean;
  disabled?: boolean;
  onClick?: () => void;
  mask?: string;
  src?: string;
  className?: string;
  text?: string;
};

export const BasicButton = (props: PropsWithChildren<_Props>) => {
  return props.tooltip ? (
    <Tooltip tooltip={props.tooltip}>
      <Button
        variant="icon"
        className={props.className}
        onClick={props.onClick}
        selected={props.selected}
        disabled={props.disabled}
        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      >
        {props.mask && <img style={{ maskImage: `url(${props.mask})` }} />}
        {props.src && <img src={props.src} />}
        {props.text && <span>{props.text}</span>}
        {props.children}
      </Button>
    </Tooltip>
  ) : (
    <Button
      className={props.className}
      onClick={props.onClick}
      selected={props.selected}
      disabled={props.disabled}
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      variant="icon"
    >
      {props.mask && <img style={{ maskImage: `url(${props.mask})` }} />}
      {props.src && <img src={props.src} />}
      {props.text && <span>{props.text}</span>}
      {props.children}
    </Button>
  );
};
