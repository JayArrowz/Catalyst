# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: Peer.proto

import sys
_b=sys.version_info[0]<3 and (lambda x:x) or (lambda x:x.encode('latin1'))
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()




DESCRIPTOR = _descriptor.FileDescriptor(
  name='Peer.proto',
  package='ADL.Peer.Protocol',
  syntax='proto3',
  serialized_options=None,
  serialized_pb=_b('\n\nPeer.proto\x12\x11\x41\x44L.Peer.Protocol\"\x1b\n\x0bPingRequest\x12\x0c\n\x04ping\x18\x01 \x01(\t\"\x1c\n\x0cPongResponse\x12\x0c\n\x04pong\x18\x02 \x01(\t\"\x1f\n\x0fPeerInfoRequest\x12\x0c\n\x04ping\x18\x01 \x01(\t\" \n\x10PeerInfoResponse\x12\x0c\n\x04pong\x18\x02 \x01(\t\"$\n\x14PeerNeighborsRequest\x12\x0c\n\x04ping\x18\x01 \x01(\t\"%\n\x15PeerNeighborsResponse\x12\x0c\n\x04pong\x18\x02 \x01(\t2\x8e\x02\n\x04Peer\x12G\n\x04Ping\x12\x1e.ADL.Peer.Protocol.PingRequest\x1a\x1f.ADL.Peer.Protocol.PongResponse\x12V\n\x0bGetPeerInfo\x12\".ADL.Peer.Protocol.PeerInfoRequest\x1a#.ADL.Peer.Protocol.PeerInfoResponse\x12\x65\n\x10GetPeerNeighbors\x12\'.ADL.Peer.Protocol.PeerNeighborsRequest\x1a(.ADL.Peer.Protocol.PeerNeighborsResponseb\x06proto3')
)




_PINGREQUEST = _descriptor.Descriptor(
  name='PingRequest',
  full_name='ADL.Peer.Protocol.PingRequest',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='ping', full_name='ADL.Peer.Protocol.PingRequest.ping', index=0,
      number=1, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=33,
  serialized_end=60,
)


_PONGRESPONSE = _descriptor.Descriptor(
  name='PongResponse',
  full_name='ADL.Peer.Protocol.PongResponse',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='pong', full_name='ADL.Peer.Protocol.PongResponse.pong', index=0,
      number=2, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=62,
  serialized_end=90,
)


_PEERINFOREQUEST = _descriptor.Descriptor(
  name='PeerInfoRequest',
  full_name='ADL.Peer.Protocol.PeerInfoRequest',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='ping', full_name='ADL.Peer.Protocol.PeerInfoRequest.ping', index=0,
      number=1, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=92,
  serialized_end=123,
)


_PEERINFORESPONSE = _descriptor.Descriptor(
  name='PeerInfoResponse',
  full_name='ADL.Peer.Protocol.PeerInfoResponse',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='pong', full_name='ADL.Peer.Protocol.PeerInfoResponse.pong', index=0,
      number=2, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=125,
  serialized_end=157,
)


_PEERNEIGHBORSREQUEST = _descriptor.Descriptor(
  name='PeerNeighborsRequest',
  full_name='ADL.Peer.Protocol.PeerNeighborsRequest',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='ping', full_name='ADL.Peer.Protocol.PeerNeighborsRequest.ping', index=0,
      number=1, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=159,
  serialized_end=195,
)


_PEERNEIGHBORSRESPONSE = _descriptor.Descriptor(
  name='PeerNeighborsResponse',
  full_name='ADL.Peer.Protocol.PeerNeighborsResponse',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  fields=[
    _descriptor.FieldDescriptor(
      name='pong', full_name='ADL.Peer.Protocol.PeerNeighborsResponse.pong', index=0,
      number=2, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=_b("").decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=197,
  serialized_end=234,
)

DESCRIPTOR.message_types_by_name['PingRequest'] = _PINGREQUEST
DESCRIPTOR.message_types_by_name['PongResponse'] = _PONGRESPONSE
DESCRIPTOR.message_types_by_name['PeerInfoRequest'] = _PEERINFOREQUEST
DESCRIPTOR.message_types_by_name['PeerInfoResponse'] = _PEERINFORESPONSE
DESCRIPTOR.message_types_by_name['PeerNeighborsRequest'] = _PEERNEIGHBORSREQUEST
DESCRIPTOR.message_types_by_name['PeerNeighborsResponse'] = _PEERNEIGHBORSRESPONSE
_sym_db.RegisterFileDescriptor(DESCRIPTOR)

PingRequest = _reflection.GeneratedProtocolMessageType('PingRequest', (_message.Message,), dict(
  DESCRIPTOR = _PINGREQUEST,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PingRequest)
  ))
_sym_db.RegisterMessage(PingRequest)

PongResponse = _reflection.GeneratedProtocolMessageType('PongResponse', (_message.Message,), dict(
  DESCRIPTOR = _PONGRESPONSE,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PongResponse)
  ))
_sym_db.RegisterMessage(PongResponse)

PeerInfoRequest = _reflection.GeneratedProtocolMessageType('PeerInfoRequest', (_message.Message,), dict(
  DESCRIPTOR = _PEERINFOREQUEST,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PeerInfoRequest)
  ))
_sym_db.RegisterMessage(PeerInfoRequest)

PeerInfoResponse = _reflection.GeneratedProtocolMessageType('PeerInfoResponse', (_message.Message,), dict(
  DESCRIPTOR = _PEERINFORESPONSE,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PeerInfoResponse)
  ))
_sym_db.RegisterMessage(PeerInfoResponse)

PeerNeighborsRequest = _reflection.GeneratedProtocolMessageType('PeerNeighborsRequest', (_message.Message,), dict(
  DESCRIPTOR = _PEERNEIGHBORSREQUEST,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PeerNeighborsRequest)
  ))
_sym_db.RegisterMessage(PeerNeighborsRequest)

PeerNeighborsResponse = _reflection.GeneratedProtocolMessageType('PeerNeighborsResponse', (_message.Message,), dict(
  DESCRIPTOR = _PEERNEIGHBORSRESPONSE,
  __module__ = 'Peer_pb2'
  # @@protoc_insertion_point(class_scope:ADL.Peer.Protocol.PeerNeighborsResponse)
  ))
_sym_db.RegisterMessage(PeerNeighborsResponse)



_PEER = _descriptor.ServiceDescriptor(
  name='Peer',
  full_name='ADL.Peer.Protocol.Peer',
  file=DESCRIPTOR,
  index=0,
  serialized_options=None,
  serialized_start=237,
  serialized_end=507,
  methods=[
  _descriptor.MethodDescriptor(
    name='Ping',
    full_name='ADL.Peer.Protocol.Peer.Ping',
    index=0,
    containing_service=None,
    input_type=_PINGREQUEST,
    output_type=_PONGRESPONSE,
    serialized_options=None,
  ),
  _descriptor.MethodDescriptor(
    name='GetPeerInfo',
    full_name='ADL.Peer.Protocol.Peer.GetPeerInfo',
    index=1,
    containing_service=None,
    input_type=_PEERINFOREQUEST,
    output_type=_PEERINFORESPONSE,
    serialized_options=None,
  ),
  _descriptor.MethodDescriptor(
    name='GetPeerNeighbors',
    full_name='ADL.Peer.Protocol.Peer.GetPeerNeighbors',
    index=2,
    containing_service=None,
    input_type=_PEERNEIGHBORSREQUEST,
    output_type=_PEERNEIGHBORSRESPONSE,
    serialized_options=None,
  ),
])
_sym_db.RegisterServiceDescriptor(_PEER)

DESCRIPTOR.services_by_name['Peer'] = _PEER

# @@protoc_insertion_point(module_scope)